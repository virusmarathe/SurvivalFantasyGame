using System;
using LiteNetLib.Utils;
using UnityEngine;

public enum PacketType : byte
{
    Serialized,
    Input
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

[Flags]
public enum MovementKeys : byte
{
    Left = 1 << 1,
    Right = 1 << 2,
    Up = 1 << 3,
    Down = 1 << 4,
    Jump = 1 << 5
}

public struct PlayerInputPacket : INetSerializable
{
    public ushort Id;
    public MovementKeys Input;
    public ushort ServerTick;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        writer.Put((byte)Input);
        writer.Put(ServerTick);
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetUShort();
        Input = (MovementKeys)reader.GetByte();
        ServerTick = reader.GetUShort();
    }
}