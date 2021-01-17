using System;
using LiteNetLib.Utils;
using UnityEngine;

public enum PacketType : byte
{
    Serialized,
    Input,
    ServerState
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
    public Vector3 Position;
    //public float Rotation;
    public ushort Tick;

    public const int Size = 1 + 12 + 2; // byte + vector3 + ushort

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Id);
        writer.Put(Position);
        //writer.Put(Rotation);
        writer.Put(Tick);
    }

    public void Deserialize(NetDataReader reader)
    {
        Id = reader.GetByte();
        Position = reader.GetVector3();
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

public struct ServerState : INetSerializable
{
    public ushort Tick;
    public ushort LastProcessedCommand;

    public int PlayerStatesCount;
    public int StartState; //server only
    public PlayerState[] PlayerStates;

    //tick
    public const int HeaderSize = sizeof(ushort) * 2;

    public void Serialize(NetDataWriter writer)
    {
        writer.Put(Tick);
        writer.Put(LastProcessedCommand);

        for (int i = StartState; i < PlayerStatesCount; i++)
        {
            PlayerStates[i].Serialize(writer);
        }
    }

    public void Deserialize(NetDataReader reader)
    {
        Tick = reader.GetUShort();
        LastProcessedCommand = reader.GetUShort();

        PlayerStatesCount = reader.AvailableBytes / PlayerState.Size;
        if (PlayerStates == null || PlayerStates.Length < PlayerStatesCount)
        {
            PlayerStates = new PlayerState[PlayerStatesCount];
        }

        for (int i = 0; i < PlayerStatesCount; i++)
        {
            PlayerStates[i].Deserialize(reader);
        }
    }
}