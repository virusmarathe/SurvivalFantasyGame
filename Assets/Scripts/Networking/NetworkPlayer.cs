using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NetworkPlayer
{
    public readonly string Name;
    public readonly byte Id;

    public Vector3 Position => _position;
    public float Speed => _speed;

    protected Vector3 _position;
    protected float _speed = 3f;

    protected NetworkPlayer(string name, byte id)
    {
        Id = id;
        Name = name;
    }

    public virtual void Spawn(Vector3 position)
    {
        _position = position;
    }

    public virtual void Update(float delta)
    {

    }

    public virtual void ApplyInput(PlayerInputPacket command, float delta)
    {
        Vector3 velocity = Vector3.zero;

        if ((command.Input & MovementKeys.Left) != 0) velocity.x = -1f;
        if ((command.Input & MovementKeys.Right) != 0) velocity.x = 1f;
        if ((command.Input & MovementKeys.Down) != 0) velocity.z = -1f;
        if ((command.Input & MovementKeys.Up) != 0) velocity.z = 1f;
        if ((command.Input & MovementKeys.Jump) != 0)
        {

        }

        _position += (velocity.normalized * _speed * delta);
    }
}
