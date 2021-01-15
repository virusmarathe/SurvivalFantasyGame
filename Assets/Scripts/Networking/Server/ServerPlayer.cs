using LiteNetLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerPlayer : NetworkPlayer
{
    public NetPeer AssociatedPeer;
    public PlayerState NetworkState;

    public ServerPlayer(string name, NetPeer peer) : base(name, (byte)peer.Id)
    {
        peer.Tag = this;
        AssociatedPeer = peer;
        NetworkState = new PlayerState { Id = (byte)peer.Id };
    }

    public override void Update(float delta)
    {
        base.Update(delta);
    }
}
