using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LiteNetLib;

public class ClientPlayer : NetworkPlayer
{
    ClientPlayerManager _playerManager;
    const float INPUT_THRESHOLD = 0.5f;
    PlayerInputPacket _nextInputPacket;
    LiteRingBuffer<PlayerInputPacket> _predictionPlayerStates;
    ServerState _lastServerState;
    const int MaxStoredCommands = 60;
    bool _firstStateReceived;
    int _updateCount;

    public Vector3 LastPosition { get; private set; }
    public int StoredCommands => _predictionPlayerStates.Count;

    public ClientPlayer(ClientPlayerManager manager, string name, byte id) : base(name, id)
    {
        _playerManager = manager;
        _predictionPlayerStates = new LiteRingBuffer<PlayerInputPacket>(MaxStoredCommands);
    }

    public override void Spawn(Vector3 position)
    {
        base.Spawn(position);
    }

    public override void Update(float delta)
    {
        base.Update(delta);

        LastPosition = _position;

        // send packet
        _nextInputPacket.Id = (ushort)((_nextInputPacket.Id + 1) % NetworkGlobals.MaxGameSequence);
        _nextInputPacket.ServerTick = _lastServerState.Tick;
        ApplyInput(_nextInputPacket, delta);

        if (_predictionPlayerStates.IsFull)
        {
            _nextInputPacket.Id = (ushort)(_lastServerState.LastProcessedCommand + 1);
            _predictionPlayerStates.FastClear();
        }
        _predictionPlayerStates.Add(_nextInputPacket);

        _updateCount++;
        if (_updateCount == 3) // send all backlog of inputs every 3rd frame
        {
            _updateCount = 0;
            foreach(PlayerInputPacket packet in _predictionPlayerStates)
            {
                NetworkClient.Instance.SendPacketSerializable<PlayerInputPacket>(PacketType.Input, packet, DeliveryMethod.Unreliable);
            }
        }
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

    public void ApplyPlayerState(ServerState serverState, PlayerState playerState)
    {
        if (!_firstStateReceived)
        {
            if (serverState.LastProcessedCommand == 0)
                return;
            _firstStateReceived = true;
        }
        if (serverState.Tick == _lastServerState.Tick || serverState.LastProcessedCommand == _lastServerState.LastProcessedCommand)
            return;

        _lastServerState = serverState;
        _position = playerState.Position;

        if (_predictionPlayerStates.Count == 0)
            return;

        ushort lastProcessedCommand = serverState.LastProcessedCommand;
        int diff = NetworkGlobals.SeqDiff(lastProcessedCommand, _predictionPlayerStates.First.Id);

        //apply prediction
        if (diff >= 0 && diff < _predictionPlayerStates.Count)
        {
            //Debug.Log($"[OK]  SP: {serverState.LastProcessedCommand}, OUR: {_predictionPlayerStates.First.Id}, DF:{diff}");
            _predictionPlayerStates.RemoveFromStart(diff + 1);
            foreach (PlayerInputPacket state in _predictionPlayerStates)
                ApplyInput(state, NetworkTimer.FixedDelta);
        }
        else if (diff >= _predictionPlayerStates.Count)
        {
            Debug.Log($"[C] Player input lag st: {_predictionPlayerStates.First.Id} ls:{lastProcessedCommand} df:{diff}");
            //lag
            _predictionPlayerStates.FastClear();
            _nextInputPacket.Id = lastProcessedCommand;
        }
        else
        {
            Debug.Log($"[ERR] SP: {serverState.LastProcessedCommand}, OUR: {_predictionPlayerStates.First.Id}, DF:{diff}, STORED: {StoredCommands}");
        }
    }
}
