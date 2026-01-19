using System;
using Shared;

namespace Integration.Tests.Base
{
    public class TestEnv
    {
        public static int DefaultIndex = 1;
        public int FrontendHttpAppIndex;
        public int FrontendSessionAppIndex;
        public int BattleAppIndex;
        public int ChatAppIndex;
        public int GateAppIndex;
        public int SponsorshipAppIndex;

        public int? ChannelId = null;

        
        public int NormalGrade = 1;
        public int SpecialGrade = 0;
        public int Level = 1;
        public int Exp = 0;

        public TestEnv CopyTo(Action<TestEnv> applyAction)
        {
            var testEnv = new TestEnv(FrontendHttpAppIndex, FrontendSessionAppIndex, BattleAppIndex, ChatAppIndex,
                GateAppIndex, SponsorshipAppIndex)
            {
                ChannelId = ChannelId,                
                NormalGrade = NormalGrade,
                SpecialGrade = SpecialGrade,
                Level = Level,
                Exp = Exp,
            };

            applyAction?.Invoke(testEnv);

            return  testEnv;
        }

        public TestEnv(int appIndex)
        {
            FrontendHttpAppIndex = appIndex;
            FrontendSessionAppIndex = appIndex;
            BattleAppIndex = appIndex;
            ChatAppIndex = appIndex;
            GateAppIndex = appIndex;
            SponsorshipAppIndex = appIndex;
        }

        public TestEnv(int frontendHttpAppIndex, int frontendSessionAppIndex, int battleAppIndex, int chatAppIndex, int gateAppIndex, int sponsorshipAppIndex)
        {
            FrontendHttpAppIndex = frontendHttpAppIndex;
            FrontendSessionAppIndex = frontendSessionAppIndex;
            BattleAppIndex = battleAppIndex;
            ChatAppIndex = chatAppIndex;
            GateAppIndex = gateAppIndex;
            SponsorshipAppIndex = sponsorshipAppIndex;
        }

        public static TestEnv Default => new(1);
        public static TestEnv S1 => new(1);
        public static TestEnv S2 => new(2);
    }
}
