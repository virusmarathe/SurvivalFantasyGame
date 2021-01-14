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
    static NetworkClient s_Instance;
    public static NetworkClient Instance { get { return s_Instance; } }

    NetManager _netManager;
    NetDataWriter _writer;
    NetPacketProcessor _packetProcessor;
    Action<DisconnectInfo> _onDisconnected;
    NetPeer _server;
    int _ping;
    string _userName;


    private void Awake()
    {
        if (s_Instance == null) s_Instance = this;
        DontDestroyOnLoad(gameObject);

        _writer = new NetDataWriter();
        _packetProcessor = new NetPacketProcessor();
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
        throw new System.NotImplementedException();
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
    }

    public void OnPeerDisconnected(NetPeer peer, DisconnectInfo disconnectInfo)
    {
        _server = null;
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
}
