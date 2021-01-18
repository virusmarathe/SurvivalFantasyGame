using LiteNetLib;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerPlayer : NetworkPlayer
{
    public NetPeer AssociatedPeer;
    public PlayerState NetworkState;
    public ushort LastProcessedCommandId { get; private set; }

    public ServerPlayer(string name, NetPeer peer) : base(name, (byte)peer.Id)
    {
        peer.Tag = this;
        AssociatedPeer = peer;
        NetworkState = new PlayerState { Id = (byte)peer.Id };
    }

    public override void Update(float delta)
    {
        base.Update(delta);
        NetworkState.Position = _position;
        NetworkState.Rotation = _rotation;
        NetworkState.Tick = LastProcessedCommandId;
    }

    public override void ApplyInput(PlayerInputPacket command, float delta)
    {
        if (NetworkGlobals.SeqDiff(command.Id, LastProcessedCommandId) <= 0) return;

        LastProcessedCommandId = command.Id;

        base.ApplyInput(command, delta);
    }
}
