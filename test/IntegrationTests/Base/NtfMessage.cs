using System;
using Shared.Model;

namespace Integration.Tests.Base
{
    public readonly struct NtfMessage<T> where T : NtfBase
    {
        public NtfMessage(int resultCode, T body = null)
        {
            ResultCode = resultCode;
            Body = body;
            BodyType = typeof(T);
        }

        public int ResultCode { get; }
        public T Body { get; }
        public Type BodyType { get; }
    }
}