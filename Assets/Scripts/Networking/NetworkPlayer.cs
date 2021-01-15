using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class NetworkPlayer
{
    public readonly string Name;
    public readonly byte Id;

    protected NetworkPlayer(string name, byte id)
    {
        Id = id;
        Name = name;
    }

    public virtual void Spawn()
    {

    }

    public virtual void Update(float delta)
    {

    }
}
