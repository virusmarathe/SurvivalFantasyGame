using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NetworkPlayer
{
    public readonly string Name;
    public readonly byte Id;

    public Vector3 Position => _position;
    public Quaternion Rotation => _rotation;
    public float Speed => _speed;

    protected Vector3 _position;
    protected Quaternion _rotation;
    protected float _speed = 3f;

    protected NetworkPlayer(string name, byte id)
    {
        Id = id;
        Name = name;
    }

    public virtual void Spawn(Vector3 position)
    {
        _position = position;
        _rotation = Quaternion.identity;
    }

    public virtual void Update(float delta)
    {

    }

    public virtual void ApplyInput(PlayerInputPacket command, float delta)
    {
        Vector3 velocity = Vector3.zero;
        float yRot = command.Rotation.eulerAngles.y * Mathf.Deg2Rad;
        Vector3 right = new Vector3(Mathf.Cos(yRot), 0, Mathf.Sin(yRot));
        Vector3 forward = new Vector3(Mathf.Sin(yRot), 0, Mathf.Cos(yRot));

        if ((command.Input & MovementKeys.Left) != 0) velocity.x = -1f;
        if ((command.Input & MovementKeys.Right) != 0) velocity.x = 1f;
        if ((command.Input & MovementKeys.Down) != 0) velocity.z = -1f;
        if ((command.Input & MovementKeys.Up) != 0) velocity.z = 1f;
        if ((command.Input & MovementKeys.Jump) != 0)
        {

        }

        forward *= velocity.z;
        right *= velocity.x;
        velocity = (forward + right).normalized;

        _position += (velocity * _speed * delta);
        _rotation = command.Rotation;
    }
}
