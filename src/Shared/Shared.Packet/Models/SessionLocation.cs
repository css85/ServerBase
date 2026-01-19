using System;
using Shared.Packet.Client.Utility;
using Shared.Packet.Extensions;

namespace Shared.Packet.Models
{
    [Serializable]
    public class SessionLocation
    {
        public static readonly SessionLocation None = new SessionLocation(SessionLocationType.None);

        // enum SessionLocationType
        public SessionLocationType Type;

        // Channel: 채널 ID
        // GameRoom: 채널 ID
        // SponsorChanceInGame: 협찬사 ID
        public int Value;

        // GameRoom: 룸 ID
        public int Value2;

        // Channel: 채널 이름
        // GameRoom: 채널 이름
        public string ValueString;

        public SessionLocation()
        {
        }

        public SessionLocation(SessionLocationType type, int value = 0, int value2 = 0, string valueString = "")
        {
            Type = type;
            Value = value;
            Value2 = value2;
            ValueString = valueString;
        }

        public SessionLocation(string data)
        {
            var separatorCount = 0;
            for (var i = 0; i < data.Length; i++)
            {
                if (data[i] == ':')
                    separatorCount++;
            }

            if (separatorCount < 3)
                return;

            var pos = 0;
            Type = (SessionLocationType)Enum.Parse(typeof(SessionLocationType),SpanUtility.ReadString(data, ':', ref pos));
            Value = SpanUtility.ReadInt(data, ':', ref pos);
            Value2 = SpanUtility.ReadInt(data, ':', ref pos);
            ValueString = SpanUtility.ReadString(data, ':', ref pos);
        }

        public override string ToString()
        {
            return $"{Type}:{Value}:{Value2}:{ValueString}";
        }
    }
}
