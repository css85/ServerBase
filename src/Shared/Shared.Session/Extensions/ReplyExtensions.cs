using System;
using System.Threading.Tasks;
using Shared.Model;
using Shared.Packet;
using Shared.Session.Data;

namespace Shared.Session.Extensions
{
    public static class ReplyExtensions
    {
        public static IReply MakeNoReply()
        {
            return null;
        }

        public static RequestReply MakeReqReply<T>() where T : RequestBase
        {
            return new(typeof(T));
        } 
        public static RequestReply MakeReqReply<T>(T data) where T : RequestBase
        {
            return new(data);
        }

        public static NtfReply MakeNtfReply<T>() where T : NtfBase
        {
            return new(typeof(T));
        }
        public static NtfReply MakeNtfReply<T>(T data) where T : NtfBase
        {
            return new(data);
        }

        public static NtfReply MakeNtfReply(Type type)
        {
            return new(type);
        }
        
        public static ResponseReply MakeResReply(Type type, ResultCode resultCode = ResultCode.Success, ushort requestId = 0)
        {
            return new(type, (int) resultCode, requestId);
        }
        public static ResponseReply MakeResReply<T>(ResultCode resultCode = ResultCode.Success, ushort requestId = 0) where T : ResponseBase
        {
            return new(typeof(T), (int) resultCode, requestId);
        }
        public static ResponseReply MakeResReply<T>(int resultCode, ushort requestId = 0) where T : ResponseBase
        {
            return new(typeof(T), resultCode, requestId);
        }
        public static ResponseReply MakeResReply<T>(T data, ResultCode resultCode = ResultCode.Success, ushort requestId = 0) where T : ResponseBase
        {
            return new(data, (int) resultCode, requestId);
        }
        public static ResponseReply MakeResReply<T>(T data, int resultCode, ushort requestId = 0) where T : ResponseBase
        {
            return new(data, resultCode, requestId);
        }

        public static Task<ResponseReply> MakeTaskResReply(Type type, ResultCode resultCode = ResultCode.Success, ushort requestId = 0)
        {
            return Task.FromResult(MakeResReply(type, resultCode, requestId));
        }
        public static Task<ResponseReply> MakeTaskResReply<T>(ResultCode resultCode = ResultCode.Success, ushort requestId = 0) where T : ResponseBase
        {
            return Task.FromResult(MakeResReply<T>(resultCode, requestId));
        }
        public static Task<ResponseReply> MakeTaskResReply<T>(int resultCode, ushort requestId = 0) where T : ResponseBase
        {
            return Task.FromResult(MakeResReply<T>(resultCode, requestId));
        }
        public static Task<ResponseReply> MakeTaskResReply<T>(T data, ResultCode resultCode = ResultCode.Success, ushort requestId = 0) where T : ResponseBase
        {
            return Task.FromResult(MakeResReply(data,resultCode,requestId));
        }
        public static Task<ResponseReply> MakeTaskResReply<T>(T data, int resultCode, ushort requestId = 0) where T : ResponseBase
        {
            return Task.FromResult(MakeResReply(data, resultCode, requestId));
        }
    }
}
