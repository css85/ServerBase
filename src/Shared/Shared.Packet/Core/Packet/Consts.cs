namespace Shared.Packet
{
    public class Const
    {
        // send = HEADER_SIZE | EXT_HEADER_SIZE
        // recv = HEADER_SIZE | RET_SIZE | EXT_HEADER_SIZE

        public const int HEADER_SIZE = TYPE_SIZE + LENGTH_SIZE;
        public const int REQ_HEADER_SIZE = TYPE_SIZE + LENGTH_SIZE + MAJOR_SIZE + MINOR_SIZE + REQUEST_ID_SIZE;
        public const int RES_HEADER_SIZE = TYPE_SIZE + LENGTH_SIZE + MAJOR_SIZE + MINOR_SIZE + REQUEST_ID_SIZE + RET_SIZE;
        public const int NTF_HEADER_SIZE = TYPE_SIZE + LENGTH_SIZE + MAJOR_SIZE + MINOR_SIZE;

        public const int BASE_HEADER_SIZE = TYPE_SIZE + LENGTH_SIZE + MAJOR_SIZE + MINOR_SIZE;

        public const int TYPE_SIZE = 1;
        public const int LENGTH_SIZE = 4;
        public const int MAJOR_SIZE = 1;
        public const int MINOR_SIZE = 1;
        public const int REQUEST_ID_SIZE = 2;
        public const int RET_SIZE = 4;
        public const int BUFFER_SIZE = 4096;
    }
}
