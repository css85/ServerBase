using CommandLine;

namespace TestConsoleApp.CommandLine
{
    [Verb("gameroom-profile", HelpText = "Test GameRoomProfile")]
    public class TestGameRoomProfileOptions:TestOptionsBase
    {
        [Option('c', "channel-id", Default = 1001, Required = true, HelpText = "Set ChannelId")]
        public int ChannelId { get; set; }


        [Option('o', "room-users", Default = 5, Required = false, HelpText = "Set user count.")]
        public int RoomUsers { get; set; }

        [Option('r', "rooms", Default = 20, Required = false, HelpText = "Set room count.")]
        public int Rooms { get; set; }

        [Option('d', "delete-delay", Default = 10, Required = false, HelpText = "Set delete room delay time seconds.")]
        public int DeleteRoomDelayTimeSeconds { get; set; }

    }
}