using Shared.Model;
using Shared.Packet;

namespace Shared
{
    public class InternalPacketResult<T> where T : class, IPacketData
    {
        public ResultCode Result;
        public T Data;

        public InternalPacketResult(int result, T data)
        {
            Result = (ResultCode) result;
            Data = data;
        }

        public InternalPacketResult(ResultCode result, T data)
        {
            Result = result;
            Data = data;
        }
    }
}
