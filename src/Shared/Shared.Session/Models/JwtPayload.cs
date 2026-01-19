namespace Shared.Session.Models
{
    public class JwtPayload
    {
        public long Seq { get; set; } // UserSeq
        public byte OsType { get; set; }    // OsType

        public short Age { get; set; }    //중복 체크용

        public JwtPayload() { }

        public JwtPayload(long seq, byte osType, short age)
        {
            Seq = seq;
            OsType = osType;
            Age = age;
        }
    }
}
