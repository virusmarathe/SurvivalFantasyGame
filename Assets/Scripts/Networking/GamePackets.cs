using System;
using LiteNetLib.Utils;
using UnityEngine;

public enum PacketType : byte
{
    Serialized
}

public class JoinPacket
{
    public string UserName { get; set; }
}

public class JoinAcceptPacket
{
    public byte Id { get; set; }
    public ushort ServerTick { get; set; }
}

public class PlayerJoinedPacket
{
    public string UserName { get; set; }
    public PlayerState InitialPlayerState { get; set; }
}

public class PlayerDisconnectedPacket
{
    public byte Id { get; set; }
}

public struct PlayerState : INetSerializable
{
    public byte Id;
    //public Vector2 Position;
    //public float Rotation;
    public ushort Tick;

    public const int Size = 1 + 2; //1 + 8 + 4 + 2;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        //writer.Put(Position);
        //writer.Put(Rotation);
        writer.Put(Tick);
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetByte();
        //Position = reader.GetVector2();
        //Rotation = reader.GetFloat();
        Tick = reader.GetUShort();
    }
}