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

    NetManager _netManager;
    NetDataWriter _writer;
    NetPacketProcessor _packetProcessor;


    private void Awake()
    {
        if (s_Instance == null) s_Instance = this;
        DontDestroyOnLoad(gameObject);

        _writer = new NetDataWriter();
        _packetProcessor = new NetPacketProcessor();
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
    }

    private void Update()
    {
        _netManager.PollEvents();
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
    }

    private void OnJoinReceived(JoinPacket joinPacket, NetPeer peer)
    {
        Debug.Log("[S] Join packet received: " + joinPacket.UserName);        
    }
}
