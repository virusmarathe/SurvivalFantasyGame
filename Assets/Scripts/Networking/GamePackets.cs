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