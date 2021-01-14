using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using System.Net;
using System.Net.Sockets;
using System;

public class NetworkClient : MonoBehaviour, INetEventListener
{
    static NetworkClient s_Instance;
    public static NetworkClient Instance { get { return s_Instance; } }

    NetManager _netManager;
    Action<DisconnectInfo> _onDisconnected;
    NetPeer _server;
    int _ping;


    private void Awake()
    {
        if (s_Instance == null) s_Instance = this;
        DontDestroyOnLoad(gameObject);

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
}
