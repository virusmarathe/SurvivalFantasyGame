using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NetworkPlayerManager : IEnumerable<NetworkPlayer>
{
    public abstract IEnumerator<NetworkPlayer> GetEnumerator();
    public abstract int Count { get; }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public abstract void NetworkUpdate();
}
