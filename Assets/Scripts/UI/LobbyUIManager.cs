using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LiteNetLib;

public class LobbyUIManager : MonoBehaviour
{
    [SerializeField] GameObject LobbyUIParent;
    [SerializeField] InputField IPField;
    [SerializeField] Text DebugText;

    private void Awake()
    {
        IPField.text = NetUtils.GetLocalIp(LocalAddrType.IPv4);
    }

    protected void OnDisconnected(DisconnectInfo info)
    {
        DebugText.text = info.Reason.ToString();
        LobbyUIParent.SetActive(true);
    }

    public void OnHostClicked()
    {
        NetworkServer.Instance.StartServer();
        NetworkClient.Instance.ConnectToServer("localhost", OnDisconnected);
        LobbyUIParent.SetActive(false);
    }

    public void OnConnectClicked()
    {
        NetworkClient.Instance.ConnectToServer(IPField.text, OnDisconnected);
        LobbyUIParent.SetActive(false);
    }
}
