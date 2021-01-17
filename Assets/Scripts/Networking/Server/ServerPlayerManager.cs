using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ServerPlayerManager : NetworkPlayerManager
{
    public override int Count => _playersCount;
    public PlayerState[] PlayerStates;

    ServerPlayer[] _players;
    int _playersCount;

    public ServerPlayerManager()
    {
        _players = new ServerPlayer[NetworkServer.MaxPlayers];
        PlayerStates = new PlayerState[NetworkServer.MaxPlayers];
    }

    public override IEnumerator<NetworkPlayer> GetEnumerator()
    {
        int i = 0;
        while (i < _playersCount)
        {
            yield return _players[i];
            i++;
        }
    }


    public void AddPlayer(ServerPlayer player)
    {
        for (int i = 0; i < _playersCount; i++)
        {
            if (_players[i].Id == player.Id)
            {
                _players[i] = player;
                return;
            }
        }

        _players[_playersCount] = player;
        _playersCount++;
    }

    public bool RemovePlayer(byte playerId)
    {
        for (int i = 0; i < _playersCount; i++)
        {
            if (_players[i].Id == playerId)
            {
                _playersCount--;
                _players[i] = _players[_playersCount];
                _players[_playersCount] = null;
                return true;
            }
        }
        return false;
    }

    public override void NetworkUpdate()
    {
        for (int i = 0; i < _playersCount; i++)
        {
            var p = _players[i];
            p.Update(NetworkTimer.FixedDelta);
            PlayerStates[i] = p.NetworkState;
        }
    }

    public bool EnableAntilag(ServerPlayer forPlayer)
    {
        return false;
    }

    public void DisableAntilag()
    {
    }
}
