using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClientPlayer : NetworkPlayer
{
    ClientPlayerManager _playerManager;

    public ClientPlayer(ClientPlayerManager manager, string name, byte id) : base(name, id)
    {
        _playerManager = manager;
    }

    //public void ReceiveServerState(ServerState serverState, PlayerState ourState)
    //{
    //}

    public override void Spawn()
    {
        base.Spawn();
    }

    public override void Update(float delta)
    {
        base.Update(delta);
    }
}
