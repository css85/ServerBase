using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using SampleGame.Shared.Extensions;
using Microsoft.Extensions.Logging;
using Shared.Network.Base;
using Shared.Server.Extensions;
using Shared.Session.Base;
using Shared.Session.Features;
using Shared.Session.Serializer;
using Shared.TcpNetwork.Base;
using Shared.TcpNetwork.Transport;

namespace Shared.Session.Services
{
    public class ServerSessionListenerService<TService> : SessionListenerBase, ITcpConnectionLogger
        where TService : ServerSessionServiceBase
    {
        private readonly ILogger<ServerSessionListenerService<TService>> _logger;
        private readonly AppContextServiceBase _appContext;
        private readonly TService _sessionService;
        private readonly ITcpSetting _tcpConnectionSettings;

        private TcpAcceptor _tcpAcceptor;

        public ServerSessionListenerService(
            ILogger<ServerSessionListenerService<TService>> logger,
            AppContextServiceBase appContext,
            TService sessionService,
            TcpConnectionSettingBase tcpSettings
            ) : base(logger, sessionService)
        {
            _logger = logger;
            _appContext = appContext;
            _sessionService = sessionService;
            _tcpConnectionSettings = tcpSettings;
        }

        public void StartListen(string host, int port)
        {
            if (_appContext.IsUnitTest)
                return;

            var ipAddress = IPAddress.Parse(host);
            _tcpAcceptor = new TcpAcceptor();
            _tcpAcceptor.Accepted += OnTcpAccepted;
            if(host == "0.0.0.0")
                ipAddress = IPAddress.Any;
            _tcpAcceptor.Listen(new IPEndPoint(ipAddress, port));
        }

        public TcpAcceptor.AcceptResult OnTcpAccepted(TcpAcceptor tcpAcceptor, Socket socket)
        {
            //유저 엑세스 토큰?
            var tcpConnection = new TcpConnection(this, _tcpConnectionSettings, socket, _sessionService.GetSessionId());
            tcpConnection.Opened += OnConnectionOpened;
            tcpConnection.Closed += OnConnectionClosed;
            tcpConnection.Received += OnReceived;
            tcpConnection.Open();

            return TcpAcceptor.AcceptResult.Accept;
        }

        public void LogTrace(string message)
        {
            _logger.LogTrace(message);
        }

        public void LogTrace(string message, Exception e)
        {
            _logger.LogTrace(e, message);
        }

        public void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        public void LogWarning(string message, Exception e)
        {
            _logger.LogWarning(e, message);
        }
    }
}