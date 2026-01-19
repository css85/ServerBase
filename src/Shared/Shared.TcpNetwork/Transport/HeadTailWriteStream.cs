using System;
using System.IO;

namespace Shared.TcpNetwork.Transport
{
    internal class HeadTailWriteStream : Stream
    {
        private Memory<byte> _head;
        private Memory<byte>? _tail;
        private int _pos;
        private int _length;
        private int _size;

        public HeadTailWriteStream(Memory<byte> head, int tailSize = 0)
        {
            _head = head;
            if (tailSize > 0)
                _tail = new Memory<byte>(new byte[tailSize]);
            _size = head.Length + tailSize;
        }

        public override bool CanRead => false;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        public override long Length => _length;

        public override long Position
        {
            get => _pos;
            set
            {
                if (value < 0)
                    throw new ArgumentOutOfRangeException("value");
                _pos = (int)value;
            }
        }

        public Memory<byte>? Tail => _tail;

        public override void Flush()
        {
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            long pos;
            switch (origin)
            {
                case SeekOrigin.Begin:
                    pos = offset;
                    break;

                case SeekOrigin.Current:
                    pos = _pos + offset;
                    break;

                case SeekOrigin.End:
                    pos = _length + offset;
                    break;

                default:
                    throw new ArgumentException("Invalid origin");
            }
            Position = pos;
            return pos;
        }

        public override void SetLength(long length)
        {
            _length = (int)length;
            EnsureCapacity(_length);
        }

        private void EnsureCapacity(int size)
        {
            if (size <= _size)
                return;

            var requiredTailSize = size - _head.Length;
            var orgTailSize = _tail != null ? _tail.Value.Length : 0;
            var newTailSize = requiredTailSize;
            if (newTailSize < 0x100)
                newTailSize = 0x100;
            if (newTailSize < orgTailSize * 2)
                newTailSize = orgTailSize * 2;

            if (_tail != null)
            {
                var newTail = new Memory<byte>(new byte[newTailSize]);
                _tail.Value.CopyTo(newTail);
                _tail = newTail;
            }
            else
            {
                _tail = new Memory<byte>(new byte[newTailSize]);
            }

            _size = _head.Length + newTailSize;
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            throw new IOException();
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            var posEnd = _pos + count;
            EnsureCapacity(posEnd);

            var headSize = _head.Length;
            if (posEnd <= headSize)
            {
                // 데이터가 모두 Head 에만 기록될 경우

                var headOffset = _pos;
                buffer.AsSpan(offset, count).CopyTo(_head.Slice(headOffset).Span);
            }
            else if (_pos >= headSize)
            {
                // 데이터가 모두 Tail 에만 기록될 경우

                var tailOffset = _pos - headSize;
                buffer.AsSpan(offset, count).CopyTo(_tail!.Value.Slice(tailOffset).Span);
            }
            else
            {
                // 데이터가 Head 와 Tail 에 나눠 기록될 경우

                var headPartCount = headSize - _pos;
                var tailPartCount = posEnd - headSize;
                buffer.AsSpan(0, headPartCount).CopyTo(_head.Slice(_pos).Span);
                buffer.AsSpan(headPartCount, tailPartCount).CopyTo(_tail!.Value.Slice(0).Span);
            }

            _pos = posEnd;
            if (_length < posEnd)
                _length = posEnd;
        }
    }
}
