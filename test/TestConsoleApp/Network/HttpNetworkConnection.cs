using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Threading.Tasks;
using SampleGame.Shared.Common;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shared;
using Shared.Model;
using Shared.Network.Base;
using Shared.Packet;
using Shared.Packet.Data;
using Shared.Packet.Extension;
using Shared.Packet.Utility;
using Shared.Server.Extensions;
using Shared.Session.Data;
using TestConsoleApp.Network.Base;

namespace TestConsoleApp.Network
{
    public class HttpConnection : INetworkConnection
    {
        private readonly NetConnectInfo _connectInfo;
        private readonly INetworkHandler _netHandler;

        private static readonly MediaTypeHeaderValue _json_media_type = new MediaTypeHeaderValue("application/json");
        private static readonly HttpClientHandler _clientHandler = new()
        {
            MaxConnectionsPerServer = int.MaxValue 
        };
        
        private static readonly HttpClient _client = new(_clientHandler,false)
        {
            Timeout = TimeSpan.FromMinutes(5),
        };

        private string _httpHost;
        private string _token;
        private AuthenticationHeaderValue _authTokenHeader;
        private readonly ILogger _logger;

        public HttpConnection(IServiceProvider services, NetConnectInfo connectInfo, INetworkHandler handler, string token)
        {
            _connectInfo = connectInfo;
            _netHandler = handler;
            _token = token;
            _logger = services.GetRequiredService<ILogger<HttpConnection>>();
        }

        public string SecretKey { get; protected set; }
        public void SetSecretKey(string secret)
        {
            SecretKey = secret;
        }
        public IConnection GetConnection() => throw new NotImplementedException("");

        public Task ConnectAsync(string host, int port)
        {
            _httpHost = $"{host}:{port}";

            if (string.IsNullOrEmpty(_token) == false)
                SetToken(_token);

            return Task.CompletedTask;
        }

        public void Close()
        {
            //_client.Dispose();
            //_client = null;
        }

        public bool IsConnected()
        {
            return _client != null;
        }

        public void SetToken(string token)
        {
            _token = token;
            _authTokenHeader = new AuthenticationHeaderValue("Bearer", _token);
        }

        public async Task SendSimpleAsync(RequestReply reply)
        {
            var sendPacketItem = reply.MakePacketItem();
            var header = PacketHeaderTable.GetHeaderData(sendPacketItem.DataType());

            if (header.HttpMethod != RequestMethodType.Post &&
                header.HttpMethod != RequestMethodType.Get)
            {
                return;
            }

            HttpResponseMessage response;
            switch (header.HttpMethod)
            {
                case RequestMethodType.Post:
                {
                    var reqMessage = new HttpRequestMessage(HttpMethod.Post, _httpHost + header.RequestUri)
                    {
                        Content = JsonContent.Create(sendPacketItem.GetData(),sendPacketItem.DataType(), _json_media_type,SystemTextJsonSerializationOptions.Default),
                        Headers =
                        {
                            Authorization = _authTokenHeader,
                        },
                    };
                    response = await _client.SendAsync(reqMessage);
                    break;
                }
                case RequestMethodType.Get:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (response.StatusCode != HttpStatusCode.OK)
            {
                HttpFail(header.Header, response);
            }
        }

        public async Task<IPacketItem> SendReceiveAsync(RequestReply reply)
        {
            var sendPacketItem = reply.MakePacketItem();
            var header = PacketHeaderTable.GetHeaderData(sendPacketItem.DataType());

            if (header.HttpMethod != RequestMethodType.Post &&
                header.HttpMethod != RequestMethodType.Get)
            {
                return Fail(header.Header, ResultCode.NotSupportedPacket, sendPacketItem.GetRequestId());
            }

            HttpResponseMessage response = null;
            
            switch (header.HttpMethod)
            {
                case RequestMethodType.Post:
                {
                    try
                    {
                        var reqMessage = new HttpRequestMessage(HttpMethod.Post, _httpHost + header.RequestUri)
                        {
                            Content = JsonContent.Create(sendPacketItem.GetData(),sendPacketItem.DataType(), _json_media_type,SystemTextJsonSerializationOptions.Default),
                            Headers =
                            {
                                Authorization = _authTokenHeader,
                            }
                        };
                        response = await _client.SendAsync(reqMessage,HttpCompletionOption.ResponseContentRead);
                    }
                    catch (Exception e)
                    {
                        _logger.LogWarning(e,$"Test Http Request({header.RequestUri} Failed !");
                        throw;
                        //Fail(header, GetResultCode(), -1);
                    }
                    break;
                }
                case RequestMethodType.Get:
                    throw new NotImplementedException();
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (response.StatusCode == HttpStatusCode.OK)
            {
                var ret = (int) ResultCode.Fail;
                if (response.Headers.TryGetValues("ret", out var rets))
                {
                    ret = int.Parse(rets.First());
                }
                var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
                return Ok(header.Header, body, ret, sendPacketItem.GetRequestId());
            }
            else
            {
                return HttpFail(header.Header, response, sendPacketItem.GetRequestId());
            }
        }

        private IPacketItem Ok(PacketHeader header, string body, int result, ushort requestId)
        {
            var responseType = PacketHeaderTable.GetResType(header);
            var headerData = PacketHeaderTable.GetHeaderData(responseType);
            var data = JsonTextSerializer.Deserialize(body, responseType) as IPacketData;

            var packetItem = new ResponsePacketItem(headerData, data, result, requestId);

            _netHandler.OnReceived(_connectInfo, packetItem, data);

            return packetItem;
        }

        private void HttpFail(PacketHeader header, HttpResponseMessage httpResponse)
        {
        }

        private IPacketItem HttpFail(PacketHeader header, HttpResponseMessage httpResponse, ushort requestId)
        {
            var responseType = PacketHeaderTable.GetResType(header);
            var headerData = PacketHeaderTable.GetHeaderData(responseType);
            var result = GetResultCode(httpResponse.StatusCode);

            var packetItem = new ResponsePacketItem(headerData, (IPacketData)null, (int) result, requestId);

            _netHandler.OnReceived(_connectInfo, packetItem,null);

            return packetItem;
        }

        private ResultCode GetResultCode(HttpStatusCode httpStatusCode)
        {
            return httpStatusCode switch
            {
                HttpStatusCode.OK => ResultCode.Success,
                HttpStatusCode.NotFound => ResultCode.NotFound,
                HttpStatusCode.Unauthorized => ResultCode.InvalidToken,
                _ => ResultCode.Fail
            };
        }

        private IPacketItem Fail(PacketHeader header, ResultCode result, ushort requestId)
        {
            var responseType = PacketHeaderTable.GetResType(header);
            var headerData = PacketHeaderTable.GetHeaderData(responseType);

            var packetItem = new ResponsePacketItem(headerData, (IPacketData)null, (int) result, requestId);

            _netHandler.OnReceived(_connectInfo, packetItem,null);

            return packetItem;
        }
    }
}
