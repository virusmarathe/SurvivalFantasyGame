using LiteNetLib.Utils;
using UnityEngine;

public static class Extensions
{
    public static void Put(this NetDataWriter writer, Vector3 vector)
    {
        writer.Put(vector.x);
        writer.Put(vector.y);
        writer.Put(vector.z);
    }

    public static Vector3 GetVector3(this NetDataReader reader)
    {
        Vector3 v;
        v.x = reader.GetFloat();
        v.y = reader.GetFloat();
        v.z = reader.GetFloat();
        return v;
    }

    public static void Put(this NetDataWriter writer, Quaternion quat)
    {
        writer.Put(quat.x);
        writer.Put(quat.y);
        writer.Put(quat.z);
        writer.Put(quat.w);
    }

    public static Quaternion GetQuaternion(this NetDataReader reader)
    {
        Quaternion q;
        q.x = reader.GetFloat();
        q.y = reader.GetFloat();
        q.z = reader.GetFloat();
        q.w = reader.GetFloat();
        return q;
    }
}