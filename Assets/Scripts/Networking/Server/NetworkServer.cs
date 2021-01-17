using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using System.Net;
using System.Net.Sockets;
using LiteNetLib.Utils;

public class NetworkServer : MonoBehaviour, INetEventListener
{
    static NetworkServer s_Instance;
    public static NetworkServer Instance { get { return s_Instance; } }

    public const int MaxPlayers = 64;

    NetManager _netManager;
    NetDataWriter _writer;
    NetPacketProcessor _packetProcessor;
    NetworkTimer _networkTimer;
    ServerPlayerManager _playerManager;
    ServerState _serverState;
    PlayerInputPacket _cachedInputCommand;

    ushort _serverTick;

    private void Awake()
    {
        if (s_Instance == null) s_Instance = this;
        DontDestroyOnLoad(gameObject);

        _networkTimer = new NetworkTimer(OnNetworkUpdate);
        _writer = new NetDataWriter();
        _playerManager = new ServerPlayerManager();
        _packetProcessor = new NetPacketProcessor();
        _serverState = new ServerState();
        _cachedInputCommand = new PlayerInputPacket();
        _packetProcessor.RegisterNestedType((w, v) => w.Put(v), r => r.GetVector3()); // register Vector3 as a serializable type
        _packetProcessor.RegisterNestedType<PlayerState>();
        _packetProcessor.SubscribeReusable<JoinPacket, NetPeer>(OnJoinReceived);

        _netManager = new NetManager(this)
        {
            AutoRecycle = true
        };
    }

    public void StartServer()
    {
        if (_netManager.IsRunning) return;

        _netManager.Start(10515);
        _networkTimer.Start();
    }

    private void Update()
    {
        _netManager.PollEvents();
        _networkTimer.Update();
    }

    private void OnDestroy()
    {
        _netManager.Stop();
        _networkTimer.Stop();
    }

    protected void OnNetworkUpdate()
    {
        _serverTick = (ushort)((_serverTick + 1) % NetworkGlobals.MaxGameSequence);
        _playerManager.NetworkUpdate();

        // send server state to clients
        if (_serverTick % 2 == 0) // send every other tick for bandwidth
        {
            _serverState.Tick = _serverTick;
            _serverState.PlayerStates = _playerManager.PlayerStates;
            
            foreach(ServerPlayer p in _playerManager)
            {
                int statesMax = p.AssociatedPeer.GetMaxSinglePacketSize(DeliveryMethod.Unreliable) - ServerState.HeaderSize;
                statesMax /= PlayerState.Size;

                _serverState.LastProcessedCommand = p.LastProcessedCommandId;
                _serverState.StartState = 0; // will need to update and handle this if packets get bigger than 1024
                _serverState.PlayerStatesCount = _playerManager.Count;
                p.AssociatedPeer.Send(WriteSerializable<ServerState>(PacketType.ServerState, _serverState), DeliveryMethod.Unreliable);
            }
        }
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        request.AcceptIfKey("SurvivalFantasyGame");
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        throw new System.NotImplementedException();
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {

    }

    public void OnNetworkReceive(NetPeer peer, NetPacketReader reader, DeliveryMethod deliveryMethod)
    {
        byte packetType = reader.GetByte();
        if (packetType >= NetworkGlobals.PacketTypesCount)
            return;
        PacketType pt = (PacketType)packetType;
        switch (pt)
        {
            case PacketType.Serialized:
                _packetProcessor.ReadAllPackets(reader, peer);
                break;
            case PacketType.Input:
                OnInputReceived(reader, peer);
                break;
            default:
                Debug.Log("Unhandled packet: " + pt);
                break;
        }
    }

    public void OnNetworkReceiveUnconnected(IPEndPoint remoteEndPoint, NetPacketReader reader, UnconnectedMessageType messageType)
    {
        throw new System.NotImplementedException();
    }

    public void OnPeerConnected(NetPeer peer)
    {
        Debug.Log("[S] Player connected: " + peer.EndPoint);
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        Debug.Log("[S] Player disconnected: " + disconnectInfo.Reason);

        if (peer.Tag != null)
        {
            byte playerId = (byte)peer.Id;
            if (_playerManager.RemovePlayer(playerId))
            {
                PlayerDisconnectedPacket pdp = new PlayerDisconnectedPacket { Id = (byte)peer.Id };
                _netManager.SendToAll(WritePacket(pdp), DeliveryMethod.ReliableOrdered);
            }
        }
    }

    private void OnJoinReceived(JoinPacket joinPacket, NetPeer peer)
    {
        Debug.Log("[S] Join packet received: " + joinPacket.UserName);
        ServerPlayer player = new ServerPlayer(joinPacket.UserName, peer);
        _playerManager.AddPlayer(player);
        player.Spawn(Vector3.zero);

        // send accept of join
        JoinAcceptPacket jap = new JoinAcceptPacket { Id = player.Id, ServerTick = _serverTick };
        peer.Send(WritePacket(jap), DeliveryMethod.ReliableOrdered);

        // notify other players of new player
        PlayerJoinedPacket pj = new PlayerJoinedPacket
        {
            UserName = joinPacket.UserName,
            InitialPlayerState = player.NetworkState
        };
        _netManager.SendToAll(WritePacket(pj), DeliveryMethod.ReliableOrdered, peer);

        // send new player other player's info
        foreach (ServerPlayer otherPlayer in _playerManager)
        {
            if (otherPlayer == player)
                continue;
            pj.UserName = otherPlayer.Name;
            pj.InitialPlayerState = otherPlayer.NetworkState;
            peer.Send(WritePacket(pj), DeliveryMethod.ReliableOrdered);
        }
    }

    private NetDataWriter WriteSerializable<T>(PacketType type, T packet) where T : struct, INetSerializable
    {
        _writer.Reset();
        _writer.Put((byte)type);
        packet.Serialize(_writer);
        return _writer;
    }

    private NetDataWriter WritePacket<T>(T packet) where T : class, new()
    {
        _writer.Reset();
        _writer.Put((byte)PacketType.Serialized);
        _packetProcessor.Write(_writer, packet);
        return _writer;
    }

    protected void OnInputReceived(NetPacketReader reader, NetPeer peer)
    {
        if (peer.Tag == null) return;

        _cachedInputCommand.Deserialize(reader);

        ServerPlayer player = (ServerPlayer)peer.Tag;
        bool antiLagUsed = _playerManager.EnableAntilag(player);
        player.ApplyInput(_cachedInputCommand, NetworkTimer.FixedDelta);
        if (antiLagUsed) _playerManager.DisableAntilag();
    }
}
