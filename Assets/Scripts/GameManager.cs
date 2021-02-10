using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] AudioSource BGMusic;

    List<Player> Players = new List<Player>();

    static GameManager s_Instance = null;
    public static GameManager Instance { get { return s_Instance; } }

    private void Awake()
    {
        if (s_Instance == null)
        {
            s_Instance = this;
        }
    }

    public void RegisterPlayer(Player p)
    {
        Players.Add(p);
        if (p.isLocalPlayer)
        {
            OnLocalPlayerStarted();
        }
    }

    void OnLocalPlayerStarted()
    {
        BGMusic.Play();
    }
}
