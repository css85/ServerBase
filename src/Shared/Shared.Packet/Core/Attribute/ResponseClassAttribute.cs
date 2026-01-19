using System;
using Shared;

/// <summary>
/// 서버코드, 클라에선 사용 안함
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class ResponseClassAttribute : Attribute
{
    byte _major;
    byte _minor;
    ProtocolType _protocolType;
    NetServiceType _serviceType;
    RequestMethodType _httpMethodType;
    string _httpPath;

    #region Property
    public byte Major { get { return _major; } }
    public byte Minor { get { return _minor; } }
    public ProtocolType ProtocolType { get { return _protocolType; } }
    public RequestMethodType HttpMethodType { get { return _httpMethodType; } }
    public string HttpPath { get { return _httpPath; } }
    public NetServiceType ServiceType { get { return _serviceType; } }
    #endregion

    /// <summary>
    /// 서버코드, 클라에선 사용 안함
    /// </summary>
    public ResponseClassAttribute(byte major, byte minor, ProtocolType protocolType, NetServiceType serviceType, RequestMethodType httpMethodType = RequestMethodType.None, string httpPostPath = default)
    {
        _major = major;
        _minor = minor;
        _protocolType = protocolType;
        _serviceType = serviceType;

        _httpMethodType = httpMethodType;
        _httpPath = httpPostPath;
    }
}
