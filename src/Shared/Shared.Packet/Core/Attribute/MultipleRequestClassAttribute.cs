using System;
using Shared;

/// <summary>
/// 서버코드, 클라에선 사용 안함
/// </summary>
// 수정시 MultipleNetServiceTypeExtensions 확인
public enum MultipleNetServiceType
{
    None,
    Web,
    WebSockets,
    Sockets,
}

/// <summary>
/// 서버코드, 클라에선 사용 안함
/// </summary>
public class NetServiceTypeInfo
{
    public readonly ProtocolType ProtocolType;
    public readonly NetServiceType NetServiceType;

    public NetServiceTypeInfo(ProtocolType protocolType, NetServiceType netServiceType)
    {
        ProtocolType = protocolType;
        NetServiceType = netServiceType;
    }
}

/// <summary>
/// 서버코드, 클라에선 사용 안함
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class MultipleRequestClassAttribute : Attribute
{
    public readonly byte Major;
    public readonly byte Minor;
    public readonly MultipleNetServiceType MultipleServiceType;

    public readonly RequestMethodType HttpMethodType;
    public readonly string HttpPath;

    /// <summary>
    /// 서버코드, 클라에선 사용 안함
    /// </summary>
    public MultipleRequestClassAttribute(byte major, byte minor, MultipleNetServiceType multipleServiceType,
        RequestMethodType httpMethodType = RequestMethodType.None, string httpPostPath = default)
    {
        Major = major;
        Minor = minor;
        MultipleServiceType = multipleServiceType;

        HttpMethodType = httpMethodType;
        HttpPath = httpPostPath;
    }
}
