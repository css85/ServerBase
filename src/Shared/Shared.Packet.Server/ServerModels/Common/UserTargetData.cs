using System;
using System.Linq;
using Shared.Server.Define;

namespace Shared.ServerModels.Common
{
    public class UserTargetData
    {
        public const char Separator = ';';
                
        public long[] Users;

        public void LoadUsers(string users)
        {
            var count = users.Count(p => p == Separator) + 1;
            Users = new long[count];

            var span = users.AsSpan();

            var userIndex = 0;
            var lastSeparatorIndex = 0;
            for (var i = 0; i < span.Length; i++)
            {
                if (span[i] == Separator)
                {
                    var userSeqSpan = span.Slice(lastSeparatorIndex, i - lastSeparatorIndex);
                    if (userSeqSpan.Length > 0)
                        Users[userIndex] = long.Parse(userSeqSpan);

                    userIndex++;
                    lastSeparatorIndex = i + 1;
                }
            }

            var lastLength = span.Length - lastSeparatorIndex;
            if (lastLength > 0)
                Users[userIndex] = long.Parse(span.Slice(lastSeparatorIndex, lastLength));

            var realLength = userIndex + 1;
            if (realLength < Users.Length)
                Users = Users.AsSpan(0, realLength).ToArray();
        }
    }
}
