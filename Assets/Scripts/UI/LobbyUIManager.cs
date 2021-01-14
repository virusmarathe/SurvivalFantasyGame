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
    }

    public void OnHostClicked()
    {
        LobbyUIParent.SetActive(false);
    }

    public void OnConnectClicked()
    {
        LobbyUIParent.SetActive(false);
    }
}
