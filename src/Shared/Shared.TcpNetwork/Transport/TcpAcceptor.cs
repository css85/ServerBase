using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace Shared.TcpNetwork.Transport
{
    public class TcpAcceptor
    {
        private Socket _socket;
        private IPEndPoint _localEndPoint;
        private Stack<SocketAsyncEventArgs> _acceptArgsPool;
        private readonly object _lock = new object();

        public bool Active => _socket != null;

        public Socket Socket => _socket;

        public IPEndPoint LocalEndPoint => _localEndPoint;

        public enum AcceptResult
        {
            Close,
            Accept
        }

        public event Func<TcpAcceptor, Socket, AcceptResult> Accepted;

        public void Listen(IPEndPoint localEndPoint, int backlog = 0x7fffffff)
        {
            if (_socket != null)
                throw new InvalidOperationException("Already Listening");

            var socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            socket.Bind(localEndPoint);
            socket.Listen(backlog);

            _socket = socket;
            _localEndPoint = localEndPoint;

            lock (_lock)
                _acceptArgsPool = new Stack<SocketAsyncEventArgs>();

            for (var i = 0; i < 10; i++)
            {
                var acceptArg = new SocketAsyncEventArgs();
                acceptArg.Completed += OnAcceptComplete;

                lock (_lock)
                    _acceptArgsPool.Push(acceptArg);
            }

            IssueAccept(0);
        }

        private void IssueAccept(int depth)
        {
            SocketAsyncEventArgs acceptArg;

            lock (_lock)
            {
                if (_acceptArgsPool.Count > 1)
                {
                    acceptArg = _acceptArgsPool.Pop();
                }
                else
                {
                    acceptArg = new SocketAsyncEventArgs();
                    acceptArg.Completed += OnAcceptComplete;
                }
            }

            if (!_socket.AcceptAsync(acceptArg))
                OnAcceptComplete(_socket, acceptArg, depth);
        }

        private void OnAcceptComplete(object sender, SocketAsyncEventArgs args)
        {
            OnAcceptComplete(sender, args, 0);
        }

        private void OnAcceptComplete(object sender, SocketAsyncEventArgs args, int depth)
        {
            var acceptSocket = args.AcceptSocket;
            args.AcceptSocket = null;

            if (_socket != null)
            {
                if (depth < 100)
                {
                    IssueAccept(depth + 1);
                }
                else
                {
                    Task.Run(() => IssueAccept(0));
                }
            }

            if (args.SocketError != SocketError.Success)
            {
                if (acceptSocket != null)
                    acceptSocket.Close();
                return;
            }

            if (acceptSocket != null)
            {
                if (Accepted == null || Accepted(this, acceptSocket) == AcceptResult.Close)
                    acceptSocket.Close();
            }

            lock (_lock)
                _acceptArgsPool.Push(args);
        }

        public void Close()
        {
            var socket = _socket;
            if (socket == null)
                return;

            _socket = null;
            _acceptArgsPool.Clear();
            _acceptArgsPool = null;

            socket.Close();
        }
    }
}
