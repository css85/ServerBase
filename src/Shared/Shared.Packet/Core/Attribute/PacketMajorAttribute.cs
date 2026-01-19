using System;
using Shared.Packet;

[AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
public class PacketMajorAttribute : Attribute
{
    public Type MinorType { get; }

    public PacketMajorAttribute(Type minorType)
    {
        MinorType = minorType;
    }
}