using System;
using Microsoft.Extensions.Logging;
using Shared.TcpNetwork.Base;

namespace Shared.Session.Extensions
{
    internal class TcpConnectionLogger : ITcpConnectionLogger
    {
        private readonly ILogger _logger;

        public TcpConnectionLogger(ILogger logger)
        {
            _logger = logger;
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

    public static class ILoggerExtensions
    {
        public static ITcpConnectionLogger CreateTcpConnectionLogger(this ILogger logger)
        {
            return new TcpConnectionLogger(logger);
        }
    }
}