using System;
using System.Threading.Tasks;
using Integration.Tests.Base;
using Shared;
using Shared.Packet;
using Shared.Session.Data;

namespace Integration.Tests.Client
{
    public interface ITestClient : IAsyncDisposable
    {
        Task InitSession();
        NetServiceType[] GetServiceTypes();
        ProtocolType GetProtocolType();
        void AuthorizeToken(string token);
        void SetEncryptKey(string key);
        void ClearReceives();
        Task<IPacketItem> WaitNtfAsync(Type type, int timeoutMilliseconds);
        Task<IPacketItem> WaitResponseAsync(ushort requestId, int timeoutMilliseconds);
        Task<SendPacketResult> SendPacketInternalAsync(RequestReply reply,bool bAssert);
    }
}
