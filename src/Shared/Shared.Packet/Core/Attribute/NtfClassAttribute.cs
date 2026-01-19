using System;
using Shared;

/// <summary>
/// 서버코드, 클라에선 사용 안함
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class NtfClassAttribute : Attribute
{
    byte _major;
    byte _minor;
    ProtocolType _protocolType;
    NetServiceType _serviceType;

    #region Property
    public byte Major { get { return _major; } }
    public byte Minor { get { return _minor; } }
    public ProtocolType ProtocolType { get { return _protocolType; } }
    public NetServiceType ServiceType { get { return _serviceType; } }
    #endregion

    /// <summary>
    /// 서버코드, 클라에선 사용 안함
    /// </summary>
    public NtfClassAttribute(byte major, byte minor, ProtocolType protocolType, NetServiceType serviceType)
    {
        _major = major;
        _minor = minor;
        _protocolType = protocolType;
        _serviceType = serviceType;
    }
}