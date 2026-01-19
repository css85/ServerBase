using System;
using Shared;

/// <summary>
/// 서버코드, 클라에선 사용 안함
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class MultipleResponseClassAttribute : Attribute
{
    public readonly byte Major;
    public readonly byte Minor;
    public readonly MultipleNetServiceType MultipleServiceType;

    public readonly RequestMethodType HttpMethodType;
    public readonly string HttpPath;

    /// <summary>
    /// 서버코드, 클라에선 사용 안함
    /// </summary>
    public MultipleResponseClassAttribute(byte major, byte minor, MultipleNetServiceType multipleServiceType,
        RequestMethodType httpMethodType = RequestMethodType.None, string httpPostPath = default)
    {
        Major = major;
        Minor = minor;
        MultipleServiceType = multipleServiceType;

        HttpMethodType = httpMethodType;
        HttpPath = httpPostPath;
    }
}
