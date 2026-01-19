using System;

/// <summary>
/// 서버코드, 클라에선 사용 안함
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
public class MultipleNtfClassAttribute : Attribute
{
    public readonly byte Major;
    public readonly byte Minor;
    public readonly MultipleNetServiceType MultipleServiceType;

    /// <summary>
    /// 서버코드, 클라에선 사용 안함
    /// </summary>
    public MultipleNtfClassAttribute(byte major, byte minor, MultipleNetServiceType multipleServiceType)
    {
        Major = major;
        Minor = minor;
        MultipleServiceType = multipleServiceType;
    }
}