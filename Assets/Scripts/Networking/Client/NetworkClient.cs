using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using System.Net;
using System.Net.Sockets;
using System;
using LiteNetLib.Utils;

public class NetworkClient : MonoBehaviour, INetEventListener
{
    [SerializeField] private ClientPlayerView _clientPlayerViewPrefab;
    [SerializeField] private RemotePlayerView _remotePlayerViewPrefab;

    static NetworkClient s_Instance;
    public static NetworkClient Instance { get { return s_Instance; } }
    public NetworkTimer NetworkTimer => _networkTimer;

    NetManager _netManager;
    NetDataWriter _writer;
    NetPacketProcessor _packetProcessor;
    Action<DisconnectInfo> _onDisconnected;
    NetPeer _server;
    NetworkTimer _networkTimer;
    ClientPlayerManager _playerManager;
    ServerState _cachedServerState;

    int _ping;
    string _userName;
    ushort _lastServerTick;

    private void Awake()
    {
        if (s_Instance == null) s_Instance = this;
        DontDestroyOnLoad(gameObject);

        _networkTimer = new NetworkTimer(OnNetworkUpdate);
        _writer = new NetDataWriter();
        _playerManager = new ClientPlayerManager();
        _cachedServerState = new ServerState();

        _packetProcessor = new NetPacketProcessor();
        _packetProcessor.RegisterNestedType((w, v) => w.Put(v), r => r.GetVector3()); // register Vector3 as a serializable type
        _packetProcessor.RegisterNestedType((w, q) => w.Put(q), r => r.GetQuaternion()); // register Quaternion
        _packetProcessor.RegisterNestedType<PlayerState>();
        _packetProcessor.SubscribeReusable<PlayerJoinedPacket>(OnPlayerJoined);
        _packetProcessor.SubscribeReusable<JoinAcceptPacket>(OnJoinAccept);
        _packetProcessor.SubscribeReusable<PlayerDisconnectedPacket>(OnPlayerDisconnected);

        _userName = Environment.MachineName + "_" + UnityEngine.Random.Range(0, 10000);

        _netManager = new NetManager(this)
        {
            AutoRecycle = true,
            IPv6Enabled = IPv6Mode.Disabled
        };
        _netManager.Start();
    }

    private void Update()
    {
        _netManager.PollEvents();
        _networkTimer.Update();
    }

    private void OnDestroy()
    {
        _netManager.Stop();
    }

    protected void OnNetworkUpdate()
    {
        _playerManager.NetworkUpdate();
    }

    protected void OnServerState()
    {
        //skip duplicate or old because we received that packet unreliably
        if (NetworkGlobals.SeqDiff(_cachedServerState.Tick, _lastServerTick) <= 0) return;
        _lastServerTick = _cachedServerState.Tick;
        _playerManager.ApplyPlayerState(ref _cachedServerState);
    }

    public void ConnectToServer(string ip, Action<DisconnectInfo> callback)
    {
        _onDisconnected = callback;
        _netManager.Connect(ip, 10515, "SurvivalFantasyGame");
    }

    public void OnConnectionRequest(ConnectionRequest request)
    {
        request.Reject(); // don't allow connections to non-server
    }

    public void OnNetworkError(IPEndPoint endPoint, SocketError socketError)
    {
        throw new System.NotImplementedException();
    }

    public void OnNetworkLatencyUpdate(NetPeer peer, int latency)
    {
        _ping = latency;
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
                _packetProcessor.ReadAllPackets(reader);
                break;
            case PacketType.ServerState:
                _cachedServerState.Deserialize(reader);
                OnServerState();
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
        Debug.Log("[C] Connected to server: " + peer.EndPoint);
        _server = peer;

        SendPacket(new JoinPacket { UserName = _userName }, DeliveryMethod.ReliableOrdered);
        _networkTimer.Start();
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        _playerManager.Clear();
        _server = null;
        _networkTimer.Stop();
        Debug.Log("[C] Disconnected from server: " + disconnectInfo.Reason);
        if (_onDisconnected != null)
        {
            _onDisconnected(disconnectInfo);
            _onDisconnected = null;
        }
    }

    public void SendPacketSerializable<T>(PacketType type, T packet, DeliveryMethod deliveryMethod) where T : INetSerializable
    {
        if (_server == null)
            return;
        _writer.Reset();
        _writer.Put((byte)type);
        packet.Serialize(_writer);
        _server.Send(_writer, deliveryMethod);
    }

    public void SendPacket<T>(T packet, DeliveryMethod deliveryMethod) where T : class, new()
    {
        if (_server == null)
            return;
        _writer.Reset();
        _writer.Put((byte)PacketType.Serialized);
        _packetProcessor.Write(_writer, packet);
        _server.Send(_writer, deliveryMethod);
    }

    protected void OnPlayerJoined(PlayerJoinedPacket packet)
    {
        Debug.Log($"[C] Player joined: {packet.UserName}");
        var remotePlayer = new RemotePlayer(packet.UserName, packet);
        var view = RemotePlayerView.Create(_remotePlayerViewPrefab, remotePlayer);
        _playerManager.AddPlayer(remotePlayer, view);
    }

    protected void OnJoinAccept(JoinAcceptPacket packet)
    {
        Debug.Log("[C] Join accept. Received player id: " + packet.Id);
        _lastServerTick = packet.ServerTick;
        var clientPlayer = new ClientPlayer(_playerManager, _userName, packet.Id);
        var view = ClientPlayerView.Create(_clientPlayerViewPrefab, clientPlayer);
        _playerManager.AddClientPlayer(clientPlayer, view);
    }

    protected void OnPlayerDisconnected(PlayerDisconnectedPacket packet)
    {
        var player = _playerManager.RemovePlayer(packet.Id);
        if (player != null)
            Debug.Log($"[C] Player disconnected: {player.Name}");
    }
}
