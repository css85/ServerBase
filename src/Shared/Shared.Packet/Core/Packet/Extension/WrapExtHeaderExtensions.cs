using System;
using System.Collections.Generic;
using System.Text;

namespace Shared.Packet.Extension
{
    public static class WrapExtHeaderExtensions
    {
        public static bool AddExtHeaderField(this WrapExtHeader o, ExtHeader key, string value)
        {
            if (o.Headers == null)
            {
                throw new NotImplementedException("[AddExtHeaderField] Error! Headers is nullptr.");
            }

            var headerKey = ExtHeaderUtility.GetHeaderKey(key);
            if (string.IsNullOrEmpty(headerKey)) return false;

            o.Headers.Add(string.Format("{0}={1}", headerKey, value));
            return true;
        }

        public static byte[] ToRawData(this WrapExtHeader o)
        {
            var stringData = o.ToStringData();
            return Encoding.UTF8.GetBytes(stringData);
        }

        public static string ToStringData(this WrapExtHeader o)
        {
            if (o.Headers == null)
            {
                throw new NotImplementedException("[AddExtHeaderField] Error! Headers is nullptr.");
            }

            return string.Join(",", o.Headers);
        }
    }
}
