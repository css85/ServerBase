using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading;
using System.Threading.Tasks;
using Integration.Tests.Base;
using Integration.Tests.Utils;
using Shared;
using Shared.Model;
using Shared.Packet;
using Shared.Packet.Data;
using Shared.Packet.Extension;
using Shared.Packet.Extensions;
using Shared.Packet.Utility;
using Shared.Server.Extensions;
using Shared.Session.Data;
using Xunit;

namespace Integration.Tests.Client
{
    public class TestGateHttpClient : ITestClient
    {
        private readonly HttpClient _httpClient;
        private readonly NetServiceType[] _netServiceTypes;

        private readonly Dictionary<ushort, IPacketItem> _responseMap = new();
        private readonly object _receiveLock = new();

        private readonly Dictionary<ushort, TaskCompletionSource<IPacketItem>> _responseWaitMap = new();

        private ushort _currentRequestId = 0;

        public ProtocolType GetProtocolType() { return ProtocolType.Http; }

        public TestGateHttpClient(HttpClient httpClient, NetServiceType[] netServiceTypes)
        {
            _httpClient = httpClient;

            _netServiceTypes = MultipleNetServiceType.Web.GetServices()
                .Where(x=>netServiceTypes.Contains(x.NetServiceType))
                .Select(p => p.NetServiceType).ToArray();
        }

        public ValueTask DisposeAsync()
        {
            _httpClient.Dispose();
            return ValueTask.CompletedTask;
        }

        public ushort GetRequestId()
        {
            _currentRequestId++;
            return _currentRequestId;
        }

        public Task InitSession()
        {
            return Task.CompletedTask;
        }

        public NetServiceType[] GetServiceTypes()
        {
            return _netServiceTypes;
        }

        public void AuthorizeToken(string token)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        
        public void ClearReceives()
        {
            lock (_receiveLock)
            {
                _responseMap.Clear();
            }
        }

        public Task<IPacketItem> WaitNtfAsync(Type type, int timeoutMilliseconds)
        {
            throw new NotSupportedException();
        }

        public async Task<IPacketItem> WaitResponseAsync(ushort requestId, int timeoutMilliseconds)
        {
            var response = GetResponse(requestId);
            if (response != null)
                return response;

            TaskCompletionSource<IPacketItem> tcs;
            lock (_receiveLock)
            {
                var cts = new CancellationTokenSource(timeoutMilliseconds);
                tcs = new TaskCompletionSource<IPacketItem>();
                cts.Token.Register(() => tcs.TrySetCanceled(), false);
                _responseWaitMap.Add(requestId, tcs);
            }

            return await tcs.Task;
        }

        public IPacketItem GetNtf(Type type)
        {
            throw new NotImplementedException();
        }

        public IPacketItem GetResponse(ushort requestId)
        {
            lock (_receiveLock)
            {
                return _responseMap.TryGetValue(requestId, out var packet) ? packet : null;
            }
        }

        public async Task<SendPacketResult> SendPacketInternalAsync(RequestReply reply,bool bAssert=true)
        {
            var packetItem = reply.MakePacketItem(GetRequestId());

            var headerData = packetItem.GetHeaderData();
            var uri = headerData.RequestUri;

            Assert.True(headerData.IsHttp());

            var httpResponse =
                await _httpClient.PostAsJsonAsync(uri, packetItem.GetData(), SystemTextJsonSerializationOptions.Default);

            httpResponse.EnsureSuccessStatusCode();

            var result = httpResponse.Headers.TryGetValues("ret", out var resultCode)
                ? int.Parse(resultCode?.FirstOrDefault() ?? "-1")
                : -1;
            
            if(bAssert)
                AssertEx.EqualResult(ResultCode.Success, result);

            var resType = PacketHeaderTable.GetResType(headerData.Header);
            var response =
                await httpResponse.Content.ReadFromJsonAsync(resType, SystemTextJsonSerializationOptions.Default);

            var responseDataType = PacketHeaderTable.GetResType(headerData.Header);
            var responseHeaderData = PacketHeaderTable.GetHeaderData(responseDataType); 
            var responsePacket = new ResponsePacketItem(responseHeaderData, (IPacketData) response,
                result, packetItem.GetRequestId());

            lock (_receiveLock)
            {
                if (_responseWaitMap.TryGetValue(packetItem.GetRequestId(), out var tcs))
                {
                    tcs.SetResult(responsePacket);
                }
                else
                {
                    _responseMap[packetItem.GetRequestId()] = responsePacket;
                }
            }

            return new SendPacketResult
            {
                SendPacketItem = packetItem,
                ResponsePacketItem = responsePacket,
            };
        }

        public void SetEncryptKey(string key)
        {
            
        }
    }
}