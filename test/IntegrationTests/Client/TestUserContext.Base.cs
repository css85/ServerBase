using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Integration.Tests.Base;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Model;
using Shared.Packet;
using Shared.Packet.Extension;
using Shared.Packet.Utility;
using Shared.PacketModel;
using static Shared.Session.Extensions.ReplyExtensions;

namespace Integration.Tests.Client
{
    public partial class TestUserContext : IAsyncDisposable
    {
        private const int SendTimeoutMilliseconds = 2000;
        private const int WaitNtfTimeoutMilliseconds = 2000;

        private readonly ILogger _logger;

        private readonly List<ITestClient> _testClients = new();
        private readonly Dictionary<NetServiceType, ITestClient> _testClientMap = new();

        public readonly TestEnv TestEnv;

        public TestUserContext(ILogger logger, TestEnv testEnv)
        {
            _logger = logger;
            TestEnv = testEnv;
        }

        public void AddTestClient(ITestClient testClient)
        {
            _testClients.Add(testClient);
            foreach (var netServiceType in testClient.GetServiceTypes())
                _testClientMap.Add(netServiceType, testClient);
        }

        public async ValueTask DisposeAsync()
        {
            for (var i = 0; i < _testClients.Count; i++)
            {
                if (_testClients[i] != null)
                    await _testClients[i].DisposeAsync();

                _testClients[i] = null;
            }
        }

        private static readonly char[] _uniqueStringChars =
            (new string(Enumerable.Range('0', '9' - '0' + 1).Select(p => (char) p).ToArray()) +
             new string(Enumerable.Range('a', 'z' - 'a' + 1).Select(p => (char) p).ToArray()) +
             new string(Enumerable.Range('A', 'Z' - 'A' + 1).Select(p => (char) p).ToArray())).ToCharArray();

        private string _userUniqueString;
        public string GetUserUniqueString()
        {
            if (_userUniqueString != null)
                return _userUniqueString;

            var uniqueString = "";
            var number = UserSeq;
            while (number > _uniqueStringChars.Length)
            {
                var r = number % _uniqueStringChars.Length;
                uniqueString = _uniqueStringChars[r] + uniqueString;
                number = number / _uniqueStringChars.Length;
            }

            _userUniqueString = _uniqueStringChars[number] + uniqueString;
            return _userUniqueString;
        }

        public void ClearResponses()
        {
            foreach (var sessionClient in _testClients)
                sessionClient.ClearReceives();
        }

        protected TestPacketResult<T> MakeResult<T>(IPacketItem response) where T : ResponseBase
        {
            return MakeResult((T)response.GetData(), response.GetResult());
        }

        protected TestPacketResult<T> MakeResult<T>(T body, ResultCode result) where T : ResponseBase
        {
            return MakeResult(body, (int) result);
        }

        protected TestPacketResult<T> MakeResult<T>(T body, int result) where T : ResponseBase
        {
            OnReceive(body, result);

            return new TestPacketResult<T>
            {
                Uc = this,
                Body = body,
                ResultCode = result,
            };
        }

        public async Task SendPacketNoReceiveAsync(RequestBase request,bool bAssert=true)
        {
            var headerData = PacketHeaderTable.GetHeaderData(request.GetType());
            _logger.LogInformation(
                "SendPacketNoReceive: [{UserSeq}], {Header}, {Type}",
                UserSeq, headerData.Header, request.GetType().Name);

            var reply = MakeReqReply(request);
            var targetTestClient = _testClientMap[headerData.ServiceType.NetServiceType];
            await targetTestClient.SendPacketInternalAsync(reply,bAssert);
        }
        
        public async Task<TestPacketResult<T>> SendPacketAsync<T>(RequestBase request,bool bAssert=false) where T : ResponseBase
        {
            var headerData = PacketHeaderTable.GetHeaderData(request.GetType());
            _logger.LogInformation(
                "Packet: Send, Success, [{UserSeq}], {Header}, {Type}, 0",
                UserSeq, headerData.Header, request.GetType().Name);

            var sw = Stopwatch.StartNew();

            var reply = MakeReqReply(request);
            var targetTestClient = _testClientMap[headerData.ServiceType.NetServiceType];
            var sendPacketResult = await targetTestClient.SendPacketInternalAsync(reply,bAssert);

            IPacketItem response;
            if (sendPacketResult.ResponsePacketItem != null)
            {
                response = sendPacketResult.ResponsePacketItem;
            }
            else
            {
                var requestId = sendPacketResult.SendPacketItem.GetRequestId();
                response = await targetTestClient.WaitResponseAsync(requestId, SendTimeoutMilliseconds);
            }

            sw.Stop();

            if (response == null)
            {
                _logger.LogInformation(
                    "Packet: Recv, Timeout, [{UserSeq}], {Header}, {Type}, {ElapsedTime}",
                    UserSeq, headerData.Header, typeof(T), sw.Elapsed);

                OnReceive(default, (int) ResultCode.Timeout);

                return new TestPacketResult<T>
                {
                    Uc = this,
                    Body = default,
                    ResultCode = (int) ResultCode.Timeout,
                };
            }

            OnReceive(response.GetData(), response.GetResult());
            if (response.GetData() is KickNtf kickNtf)
            {
                _logger.LogInformation(
                    "Packet: Recv, [{UserSeq}], {Header}, {Type}, {ElapsedTime}",
                    UserSeq, headerData.Header, typeof(T), sw.Elapsed);

                return new TestPacketResult<T>
                {
                    Uc = this,
                    Body = null,
                    ResultCode = 0,
                };
            }

            _logger.LogInformation(
                "Packet: Recv, Success, [{UserSeq}], {Header}, {Type}, {ElapsedTime}",
                UserSeq, headerData.Header, typeof(T), sw.Elapsed);

            return new TestPacketResult<T>
            {
                Uc = this,
                Body = (T) response.GetData(),
                ResultCode = response.GetResult(),
            };
        }

        public async Task<MultipleTestPacketResult<TMain, TNtf>> SendPacketWithNtfAsync<TMain, TNtf>(
            TestUserContext[] ntfTargets, RequestBase packet, int ntfTimeout = 5)
            where TMain : ResponseBase
            where TNtf : NtfBase
        {
            var ntfResults = new NtfMessage<TNtf>[ntfTargets.Length];
            var ntfWaiters = ntfTargets.Select(async (p, i) =>
            {
                ntfResults[i] = await p.WaitNtfReceiveAsync<TNtf>(ntfTimeout);
            });

            var mainResult = await SendPacketAsync<TMain>(packet);
            await Task.WhenAll(ntfWaiters);

            return new MultipleTestPacketResult<TMain, TNtf>
            {
                Main = mainResult,
                Ntf = ntfTargets.Select((p, i) => new TestNtfPacketResult<TNtf>
                {
                    Uc = p,
                    ResultCode = ntfResults[i].ResultCode,
                    Body = ntfResults[i].Body,
                }).ToArray()
            };
        }

        public async Task<MultipleTestPacketResult<TMain, TNtf, TNtf2>> SendPacketWithNtfAsync<TMain, TNtf, TNtf2>(
            TestUserContext[] ntfTargets, TestUserContext[] ntfTargets2, RequestBase packet)
            where TMain : ResponseBase
            where TNtf : NtfBase
            where TNtf2 : NtfBase
        {
            var ntfResults = new NtfMessage<TNtf>[ntfTargets.Length];
            var ntfWaiters = ntfTargets.Select(async (p, i) =>
            {
                ntfResults[i] = await p.WaitNtfReceiveAsync<TNtf>();
            });
            var ntfResults2 = new NtfMessage<TNtf2>[ntfTargets2.Length];
            var ntfWaiters2 = ntfTargets2.Select(async (p, i) =>
            {
                ntfResults2[i] = await p.WaitNtfReceiveAsync<TNtf2>();
            });

            var mainResult = await SendPacketAsync<TMain>(packet);

            await Task.WhenAll(ntfWaiters.Concat(ntfWaiters2));

            return new MultipleTestPacketResult<TMain, TNtf, TNtf2>
            {
                Main = mainResult,
                Ntf0 = ntfTargets.Select((p, i) => new TestNtfPacketResult<TNtf>
                {
                    Uc = p,
                    ResultCode = ntfResults[i].ResultCode,
                    Body = ntfResults[i].Body,
                }).ToArray(),
                Ntf1 = ntfTargets2.Select((p, i) => new TestNtfPacketResult<TNtf2>
                {
                    Uc = p,
                    ResultCode = ntfResults2[i].ResultCode,
                    Body = ntfResults2[i].Body,
                }).ToArray()
            };
        }

        public async Task<MultipleTestPacketResult<TMain, TNtf, TNtf2, TNtf3>>
            SendPacketWithNtfAsync<TMain, TNtf, TNtf2, TNtf3>(TestUserContext[] ntfTargets,
                TestUserContext[] ntfTargets2, TestUserContext[] ntfTargets3, RequestBase packet)
            where TMain : ResponseBase
            where TNtf : NtfBase
            where TNtf2 : NtfBase
            where TNtf3 : NtfBase
        {
            var ntfResults = new NtfMessage<TNtf>[ntfTargets.Length];
            var ntfWaiters = ntfTargets.Select(async (p, i) =>
            {
                ntfResults[i] = await p.WaitNtfReceiveAsync<TNtf>();
            });
            var ntfResults2 = new NtfMessage<TNtf2>[ntfTargets2.Length];
            var ntfWaiters2 = ntfTargets2.Select(async (p, i) =>
            {
                ntfResults2[i] = await p.WaitNtfReceiveAsync<TNtf2>();
            });
            var ntfResults3 = new NtfMessage<TNtf3>[ntfTargets3.Length];
            var ntfWaiters3 = ntfTargets3.Select(async (p, i) =>
            {
                ntfResults3[i] = await p.WaitNtfReceiveAsync<TNtf3>();
            });

            var mainResult = await SendPacketAsync<TMain>(packet);

            await Task.WhenAll(ntfWaiters.Concat(ntfWaiters2).Concat(ntfWaiters3));

            return new MultipleTestPacketResult<TMain, TNtf, TNtf2, TNtf3>
            {
                Main = mainResult,
                Ntf0 = ntfTargets.Select((p, i) => new TestNtfPacketResult<TNtf>
                {
                    Uc = p,
                    ResultCode = ntfResults[i].ResultCode,
                    Body = ntfResults[i].Body,
                }).ToArray(),
                Ntf1 = ntfTargets2.Select((p, i) => new TestNtfPacketResult<TNtf2>
                {
                    Uc = p,
                    ResultCode = ntfResults2[i].ResultCode,
                    Body = ntfResults2[i].Body,
                }).ToArray(),
                Ntf2 = ntfTargets3.Select((p, i) => new TestNtfPacketResult<TNtf3>
                {
                    Uc = p,
                    ResultCode = ntfResults3[i].ResultCode,
                    Body = ntfResults3[i].Body,
                }).ToArray()
            };
        }

        public async Task<MultipleTestPacketResult<TMain, TNtf, TNtf2, TNtf3, TNtf4>>
            SendPacketWithNtfAsync<TMain, TNtf, TNtf2, TNtf3, TNtf4>(TestUserContext[] ntfTargets,
                TestUserContext[] ntfTargets2, TestUserContext[] ntfTargets3, TestUserContext[] ntfTargets4,
                RequestBase packet)
            where TMain : ResponseBase
            where TNtf : NtfBase
            where TNtf2 : NtfBase
            where TNtf3 : NtfBase
            where TNtf4 : NtfBase
        {
            var ntfResults = new NtfMessage<TNtf>[ntfTargets.Length];
            var ntfWaiters = ntfTargets.Select(async (p, i) =>
            {
                ntfResults[i] = await p.WaitNtfReceiveAsync<TNtf>();
            });
            var ntfResults2 = new NtfMessage<TNtf2>[ntfTargets2.Length];
            var ntfWaiters2 = ntfTargets2.Select(async (p, i) =>
            {
                ntfResults2[i] = await p.WaitNtfReceiveAsync<TNtf2>();
            });
            var ntfResults3 = new NtfMessage<TNtf3>[ntfTargets3.Length];
            var ntfWaiters3 = ntfTargets3.Select(async (p, i) =>
            {
                ntfResults3[i] = await p.WaitNtfReceiveAsync<TNtf3>();
            });
            var ntfResults4 = new NtfMessage<TNtf4>[ntfTargets4.Length];
            var ntfWaiters4 = ntfTargets4.Select(async (p, i) =>
            {
                ntfResults4[i] = await p.WaitNtfReceiveAsync<TNtf4>();
            });

            var mainResult = await SendPacketAsync<TMain>(packet);

            await Task.WhenAll(ntfWaiters.Concat(ntfWaiters2).Concat(ntfWaiters3).Concat(ntfWaiters4));

            return new MultipleTestPacketResult<TMain, TNtf, TNtf2, TNtf3, TNtf4>
            {
                Main = mainResult,
                Ntf0 = ntfTargets.Select((p, i) => new TestNtfPacketResult<TNtf>
                {
                    Uc = p,
                    ResultCode = ntfResults[i].ResultCode,
                    Body = ntfResults[i].Body,
                }).ToArray(),
                Ntf1 = ntfTargets2.Select((p, i) => new TestNtfPacketResult<TNtf2>
                {
                    Uc = p,
                    ResultCode = ntfResults2[i].ResultCode,
                    Body = ntfResults2[i].Body,
                }).ToArray(),
                Ntf2 = ntfTargets3.Select((p, i) => new TestNtfPacketResult<TNtf3>
                {
                    Uc = p,
                    ResultCode = ntfResults3[i].ResultCode,
                    Body = ntfResults3[i].Body,
                }).ToArray(),
                Ntf3 = ntfTargets4.Select((p, i) => new TestNtfPacketResult<TNtf4>
                {
                    Uc = p,
                    ResultCode = ntfResults4[i].ResultCode,
                    Body = ntfResults4[i].Body,
                }).ToArray(),
            };
        }

        public async Task<MultipleTestPacketResult<TMain, TNtf, TNtf2, TNtf3, TNtf4, TNtf5>>
            SendPacketWithNtfAsync<TMain, TNtf, TNtf2, TNtf3, TNtf4, TNtf5>(TestUserContext[] ntfTargets,
                TestUserContext[] ntfTargets2, TestUserContext[] ntfTargets3, TestUserContext[] ntfTargets4,
                TestUserContext[] ntfTargets5, RequestBase packet)
            where TMain : ResponseBase
            where TNtf : NtfBase
            where TNtf2 : NtfBase
            where TNtf3 : NtfBase
            where TNtf4 : NtfBase
            where TNtf5 : NtfBase
        {
            var ntfResults = new NtfMessage<TNtf>[ntfTargets.Length];
            var ntfWaiters = ntfTargets.Select(async (p, i) =>
            {
                ntfResults[i] = await p.WaitNtfReceiveAsync<TNtf>();
            });
            var ntfResults2 = new NtfMessage<TNtf2>[ntfTargets2.Length];
            var ntfWaiters2 = ntfTargets2.Select(async (p, i) =>
            {
                ntfResults2[i] = await p.WaitNtfReceiveAsync<TNtf2>();
            });
            var ntfResults3 = new NtfMessage<TNtf3>[ntfTargets3.Length];
            var ntfWaiters3 = ntfTargets3.Select(async (p, i) =>
            {
                ntfResults3[i] = await p.WaitNtfReceiveAsync<TNtf3>();
            });
            var ntfResults4 = new NtfMessage<TNtf4>[ntfTargets4.Length];
            var ntfWaiters4 = ntfTargets4.Select(async (p, i) =>
            {
                ntfResults4[i] = await p.WaitNtfReceiveAsync<TNtf4>();
            });
            var ntfResults5 = new NtfMessage<TNtf5>[ntfTargets5.Length];
            var ntfWaiters5 = ntfTargets5.Select(async (p, i) =>
            {
                ntfResults5[i] = await p.WaitNtfReceiveAsync<TNtf5>();
            });

            var mainResult = await SendPacketAsync<TMain>(packet);

            await Task.WhenAll(ntfWaiters.Concat(ntfWaiters2).Concat(ntfWaiters3).Concat(ntfWaiters4)
                .Concat(ntfWaiters5));

            return new MultipleTestPacketResult<TMain, TNtf, TNtf2, TNtf3, TNtf4, TNtf5>
            {
                Main = mainResult,
                Ntf0 = ntfTargets.Select((p, i) => new TestNtfPacketResult<TNtf>
                {
                    Uc = p,
                    ResultCode = ntfResults[i].ResultCode,
                    Body = ntfResults[i].Body,
                }).ToArray(),
                Ntf1 = ntfTargets2.Select((p, i) => new TestNtfPacketResult<TNtf2>
                {
                    Uc = p,
                    ResultCode = ntfResults2[i].ResultCode,
                    Body = ntfResults2[i].Body,
                }).ToArray(),
                Ntf2 = ntfTargets3.Select((p, i) => new TestNtfPacketResult<TNtf3>
                {
                    Uc = p,
                    ResultCode = ntfResults3[i].ResultCode,
                    Body = ntfResults3[i].Body,
                }).ToArray(),
                Ntf3 = ntfTargets4.Select((p, i) => new TestNtfPacketResult<TNtf4>
                {
                    Uc = p,
                    ResultCode = ntfResults4[i].ResultCode,
                    Body = ntfResults4[i].Body,
                }).ToArray(),
                Ntf4 = ntfTargets5.Select((p, i) => new TestNtfPacketResult<TNtf5>
                {
                    Uc = p,
                    ResultCode = ntfResults5[i].ResultCode,
                    Body = ntfResults5[i].Body,
                }).ToArray(),
            };
        }

        public async Task<NtfMessage<T>> WaitNtfReceiveAsync<T>(int timeoutMilliseconds = WaitNtfTimeoutMilliseconds)
            where T : NtfBase
        {
            var headerData = PacketHeaderTable.GetHeaderData(typeof(T));
            var targetTestClient = _testClientMap[headerData.ServiceType.NetServiceType];

            var sw = Stopwatch.StartNew();

            var ntf = await targetTestClient.WaitNtfAsync(typeof(T), timeoutMilliseconds);
            if (ntf == null)
            {
                _logger.LogInformation("NTF_Timeout: [{UserSeq}], {Type} | {ElapsedTime}",
                    UserSeq, typeof(T), sw.Elapsed);
                OnReceive(default, (int)ResultCode.Timeout);
                return new NtfMessage<T>((int) ResultCode.Timeout);
            }

            _logger.LogInformation(
                "RecvNTFPacket: [{UserSeq}], {Header} | {ElapsedTime}", UserSeq, ntf.Header(), sw.Elapsed);

            OnReceive(ntf.GetData(), ntf.GetResult());
            return new NtfMessage<T>(ntf.GetResult(), (T) ntf.GetData());
        }
    }
}