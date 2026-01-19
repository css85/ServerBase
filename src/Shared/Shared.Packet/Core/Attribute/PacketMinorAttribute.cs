using System;
using Shared.Packet;

[AttributeUsage(AttributeTargets.Enum, AllowMultiple = false, Inherited = true)]
public class PacketMinorAttribute : Attribute
{
    public MAJOR Major { get; }

    public PacketMinorAttribute(MAJOR major)
    {
        Major = major;
    }
}