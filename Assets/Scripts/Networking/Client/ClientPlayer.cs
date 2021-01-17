using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;

public class ClientPlayer : NetworkPlayer
{
    ClientPlayerManager _playerManager;
    const float INPUT_THRESHOLD = 0.5f;
    PlayerInputPacket _nextInputPacket;

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

        // send packet?
        //NetworkClient.Instance.SendPacketSerializable<PlayerInputPacket>(PacketType.Input, _nextInputPacket, DeliveryMethod.Unreliable);
    }

    public void SetInput(float xVal, float yVal, float jumpVal)
    {
        _nextInputPacket.Input = 0;

        if (xVal >= INPUT_THRESHOLD)
        {
            _nextInputPacket.Input |= MovementKeys.Right;
        }
        if (xVal <= -INPUT_THRESHOLD)
        {
            _nextInputPacket.Input |= MovementKeys.Left;
        }
        if (yVal >= INPUT_THRESHOLD)
        {
            _nextInputPacket.Input |= MovementKeys.Up;
        }
        if (yVal <= -INPUT_THRESHOLD)
        {
            _nextInputPacket.Input |= MovementKeys.Down;
        }
        if (jumpVal >= INPUT_THRESHOLD)
        {
            _nextInputPacket.Input |= MovementKeys.Jump;
        }
    }
}
