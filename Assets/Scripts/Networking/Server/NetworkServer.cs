using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;
using System.Net;
using System.Net.Sockets;

public class NetworkServer : MonoBehaviour, INetEventListener
{
    static NetworkServer s_Instance;
    public static NetworkServer Instance { get { return s_Instance; } }

    NetManager _netManager;

    private void Awake()
    {
        if (s_Instance == null) s_Instance = this;
        DontDestroyOnLoad(gameObject);

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
        throw new System.NotImplementedException();
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
}
