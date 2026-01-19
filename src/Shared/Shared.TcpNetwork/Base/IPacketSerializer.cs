using System.IO;

namespace Shared.TcpNetwork.Base
{
    public interface IPacketSerializer
    {
        // 지정한 패킷이 직렬화 되었을 때의 길이를 구해 반환합니다.
        // 만약 알 수 없다면 0 을 반환합니다.
        // 이 정보는 버퍼 관리를 위한 힌트로만 사용합니다.
        int EstimateLength(object packet);

        void Serialize(Stream stream, object packet);

        byte[] Serialize(object packet);

        // Deserialize 에 필요한 버퍼 크기가 얼마인지 반환합니다.
        // 만약 알 수 없다면 0 을 반환합니다.
        int PeekLength(Stream stream);

        object Deserialize(Stream stream);
    }
}
