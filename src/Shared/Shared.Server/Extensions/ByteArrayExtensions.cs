namespace SampleGame.Shared.Extensions
{
    public static class ByteArrayExtensions
    {
        public static int Compare(this byte[] lhs, byte[] rhs)
        {
            if (lhs == null)
            {
                if (rhs == null)
                    return 0;
                return 1;
            }

            if (rhs == null)
                return -1;

            if (lhs.Length > rhs.Length)
                return -1;

            if (lhs.Length < rhs.Length)
                return 1;

            for (var i = 0; i < lhs.Length; i++)
            {
                if (lhs[i] > rhs[i])
                    return -1;
                if (lhs[i] < rhs[i])
                    return 1;
            }

            return 0;
        }
    }
}