using System;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Shared.Network.Base;
using Shared.Network.Utility;
using Shared.Session.Base;
using Shared.Session.Features;
using Shared.TcpNetwork.Base;
using Shared.TcpNetwork.Transport;
using Shared.Utility;

namespace Shared.Session.Services
{
    public class UserSessionListenerService<TService> : SessionListenerBase, ITcpConnectionLogger
        where TService : UserSessionServiceBase
    {
        private readonly ILogger<UserSessionListenerService<TService>> _logger;
        private readonly AppContextServiceBase _appContext;
        private readonly TService _sessionService;
        private readonly ITcpSetting _tcpConnectionSettings;
        private readonly PacketResolverService _packetResolverService;

        private TcpAcceptor _tcpAcceptor;
        public UserSessionListenerService(
            ILogger<UserSessionListenerService<TService>> logger,
            AppContextServiceBase appContext,
            TService sessionService,
            TcpEncryptConnectionSettings tcpSettings,
            PacketResolverService packetResolverService) : base(logger, sessionService)
        {
            _logger = logger;
            _appContext = appContext;
            _sessionService = sessionService;
            _tcpConnectionSettings = tcpSettings;
            _packetResolverService = packetResolverService;
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
            
            var tcpConnection = _appContext.IsUnitTest?new TcpConnection(this, _tcpConnectionSettings, socket, _sessionService.GetSessionId()):new TcpConnection(this, _tcpConnectionSettings, socket, _sessionService.GetSessionId(),EncryptProvider.CreateKey());
            tcpConnection.Opened += OnConnectionOpened;
            tcpConnection.Closed += OnConnectionClosed;
            tcpConnection.Received += OnReceived;
            tcpConnection.Authenticated += OnAuthenticated;
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

        protected override void OnAuthenticated(IConnection connection)
        {
            base.OnAuthenticated(connection);
            connection.IsAuthenticated = true;
        }

        public override async Task OnReceivedAsync(PacketWorkerItem item)
        {
            try
            {
                var session = _sessionService.GetSession(item.Id);
                if (session == null)
                {
                    _logger.LogWarning("SessionListenerBase: Session not found: SessionType({SessionType}), SessionId({SessionId})",
                        _sessionService.SessionType.Name, item.Id);
                    return;
                }

                if (item.Packet == null)
                {
                    if (item.Extra == 0)
                    {
                        if (session.SessionService.IsUserSession)
                        {
                            await CloseUserSessionAsync(session);
                            return;
                        }
                        else
                        {
                            await _sessionService.OnSessionClosedAsync(session);
                            return;
                        }
                    }
                    else
                    {
                        await session.KickAsync();
                        return; 
                    }
                }

                var packetItem  = _packetResolverService.ResolverPacketItem(session, item.Packet);

                await session.OnReceiveAsync(packetItem);

                if(_sessionService.SessionEnterPacketType == packetItem.GetHeaderData().DataType)
                    OnAuthenticated(session.Connection);
            }
            catch (Exception e)
            {
                _logger.LogCritical(e, "OnReceivedAsync: Exception");
                throw;
            }
        }
    }
}