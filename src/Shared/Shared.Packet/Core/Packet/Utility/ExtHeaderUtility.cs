using System;
using System.Collections.Generic;
using Shared.Packet.Extension;

namespace Shared.Packet
{
    public class ExtHeaderUtility
    {
        static readonly Dictionary<int, string> _extHeaderMetaData = new Dictionary<int, string>
        {
            { (int)ExtHeader.Auth , "Authorization"} //Bearer token
            ,{ (int)ExtHeader.Custom , "X-Ext-Custom"}
            ,{ (int)ExtHeader.Session , "X-Ext-Session"}
            ,{ (int)ExtHeader.Test1 , "X-Ext-Test1"}
        };

        public static bool ContainsHeaderKey(ExtHeader header)
        {
            int key = (int)header;
            return _extHeaderMetaData.ContainsKey(key);
        }

        public static string GetHeaderKey(ExtHeader header)
        {
            int key = (int)header;
            if (_extHeaderMetaData.ContainsKey(key))
                return _extHeaderMetaData[key];

            return null;
        }
    }
}