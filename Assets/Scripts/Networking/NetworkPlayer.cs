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
    protected float _speed = 5f;
    protected bool _grounded = true;

    float _fallingTimer = 0f;
    float _initVelocity = 0f;

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
        Vector3 forward = new Vector3(Mathf.Sin(yRot), 0, Mathf.Cos(yRot));
        Vector3 right = Vector3.Cross(Vector3.up, forward);

        if ((command.Input & MovementKeys.Left) != 0) velocity.x = -1f;
        if ((command.Input & MovementKeys.Right) != 0) velocity.x = 1f;
        if ((command.Input & MovementKeys.Down) != 0) velocity.z = -1f;
        if ((command.Input & MovementKeys.Up) != 0) velocity.z = 1f;
        if ((command.Input & MovementKeys.Jump) != 0)
        {
            if (_grounded)
            {
                _grounded = false;
                _initVelocity = 4;
            }
        }

        forward *= velocity.z;
        right *= velocity.x;
        velocity = (forward + right);
        velocity = velocity.normalized;

        if (!_grounded)
        {
            velocity.y += _initVelocity  + (- 9.8f * _fallingTimer);
        }

        _position += (velocity * _speed * delta);
        _rotation = command.Rotation;

        RaycastHit hit;
        if (Physics.Raycast(_position + new Vector3(0,2,0), new Vector3(0, -1, 0), out hit, 2.6f, 1 << LayerMask.NameToLayer("Terrain")))
        {
            _position = new Vector3(_position.x, hit.point.y + 0.6f, _position.z);
            _grounded = true;
            _fallingTimer = 0;
            _initVelocity = 0;
        }
        else
        {
            _grounded = false;
            _fallingTimer += delta;
        }
    }
}
