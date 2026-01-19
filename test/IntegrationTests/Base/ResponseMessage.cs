using System;
using Shared.Model;

namespace Integration.Tests.Base
{
    public readonly struct ResponseMessage<T> where T : ResponseBase
    {
        public ResponseMessage(int resultCode, T body = null)
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
