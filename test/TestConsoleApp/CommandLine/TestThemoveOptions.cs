using CommandLine;

namespace TestConsoleApp.CommandLine
{
    [Verb("themove", HelpText = "Test TheMove")]
    public class TestThemoveOptions :TestOptionsBase
    {
        [Option("startid", Default = 1, Required = true, HelpText = "Set Start ID.")]
        public int StartID { get; set; }

        [Option("createuser", Default = true, Required = true, HelpText = "Is Create User")]
        public bool IsCreateUser { get; set; }

        [Option("validcheck", Default = true, Required = true, HelpText = "Valid Check")]
        public bool IsValidCheck { get; set; }

        [Option("creategameroomrandomvalue", Default = 10, Required = true, HelpText = "Create Game Room Random Value")]
        public int CreateGameroomRoomRandomValue { get; set; }

        [Option("joingameroomrandomvalue", Default = 3, Required = true, HelpText = "Join Game Room Random Value")]
        public int JoinGameroomRoomRandomValue { get; set; }

        [Option("leavegameroomrandomvalue", Default = 10, Required = true, HelpText = "Leave Game Room Random Value")]
        public int LeaveGameRoomRandomValue { get; set; }
    }
}