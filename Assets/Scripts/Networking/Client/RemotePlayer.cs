using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RemotePlayer : NetworkPlayer
{
    LiteRingBuffer<PlayerState> _buffer = new LiteRingBuffer<PlayerState>(30);
    float _receivedTime;
    const float BufferTime = 0.1f;
    float _timer;

    public RemotePlayer(string name, PlayerJoinedPacket pjPacket) : base(name, pjPacket.InitialPlayerState.Id)
    {
        _position = pjPacket.InitialPlayerState.Position;
        _rotation = pjPacket.InitialPlayerState.Rotation;
        _buffer.Add(pjPacket.InitialPlayerState);
    }

    public override void Spawn(Vector3 position)
    {
        _buffer.FastClear();
        base.Spawn(position);
    }

    public void UpdatePosition(float delta)
    {
        // read from buffer, and lerp between top two values
        if (_receivedTime < BufferTime || _buffer.Count < 2)
            return;
        var dataA = _buffer[0];
        var dataB = _buffer[1];

        float lerpTime = NetworkGlobals.SeqDiff(dataB.Tick, dataA.Tick) * NetworkTimer.FixedDelta;
        float t = _timer / lerpTime;
        _position = Vector3.Lerp(dataA.Position, dataB.Position, t);
        _rotation = Quaternion.Slerp(dataA.Rotation, dataB.Rotation, t);
        _timer += delta;
        if (_timer > lerpTime)
        {
            _receivedTime -= lerpTime;
            _buffer.RemoveFromStart(1);
            _timer -= lerpTime;
        }
    }

    public void OnPlayerState(PlayerState state)
    {
        int diff = NetworkGlobals.SeqDiff(state.Tick, _buffer.Last.Tick);
        if (diff <= 0)
            return;

        _receivedTime += diff * NetworkTimer.FixedDelta;
        if (_buffer.IsFull)
        {
            Debug.LogWarning("[C] Remote: Something happened");
            //Lag?
            _receivedTime = 0f;
            _buffer.FastClear();
        }
        _buffer.Add(state);
    }
}
