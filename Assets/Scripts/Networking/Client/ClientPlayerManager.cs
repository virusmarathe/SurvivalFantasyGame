using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct PlayerHandler
{
    public readonly NetworkPlayer Player;
    public readonly IPlayerView View;

    public PlayerHandler(NetworkPlayer player, IPlayerView view)
    {
        Player = player;
        View = view;
    }

    public void Update(float delta)
    {
        Player.Update(delta);
    }
}

public class ClientPlayerManager : NetworkPlayerManager
{
    public ClientPlayer OurPlayer => _clientPlayer;
    public override int Count => _players.Count;

    Dictionary<byte, PlayerHandler> _players;
    ClientPlayer _clientPlayer;

    public ClientPlayerManager()
    {
        _players = new Dictionary<byte, PlayerHandler>();
    }

    public override IEnumerator<NetworkPlayer> GetEnumerator()
    {
        foreach (var ph in _players)
            yield return ph.Value.Player;
    }

    public override void NetworkUpdate()
    {
        foreach (var kv in _players)
            kv.Value.Update(NetworkTimer.FixedDelta);
    }

    public NetworkPlayer GetById(byte id)
    {
        return _players.TryGetValue(id, out var ph) ? ph.Player : null;
    }

    public NetworkPlayer RemovePlayer(byte id)
    {
        if (_players.TryGetValue(id, out var handler))
        {
            _players.Remove(id);
            handler.View.Destroy();
        }

        return handler.Player;
    }

    public void AddClientPlayer(ClientPlayer player, ClientPlayerView view)
    {
        _clientPlayer = player;
        _players.Add(player.Id, new PlayerHandler(player, view));
    }

    public void AddPlayer(RemotePlayer player, RemotePlayerView view)
    {
        _players.Add(player.Id, new PlayerHandler(player, view));
    }

    public void Clear()
    {
        foreach (var p in _players.Values)
            p.View.Destroy();
        _players.Clear();
    }
}
