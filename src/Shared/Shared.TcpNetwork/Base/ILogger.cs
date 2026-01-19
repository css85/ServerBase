using System;

namespace Shared.TcpNetwork.Base
{
    public interface ITcpConnectionLogger
    {
        void LogTrace(string message);
        void LogTrace(string message, Exception e);
        void LogWarning(string message);
        void LogWarning(string message, Exception e);
    }
}