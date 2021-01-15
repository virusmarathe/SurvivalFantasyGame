using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayer : NetworkPlayer
{

    public RemotePlayer(string name, PlayerJoinedPacket pjPacket) : base(name, pjPacket.InitialPlayerState.Id)
    {
    }

    public override void Spawn()
    {
        base.Spawn();
    }

    //public void UpdatePosition(float delta)
    //{
        
    //}

    //public void OnPlayerState(PlayerState state)
    //{

    //}
}
