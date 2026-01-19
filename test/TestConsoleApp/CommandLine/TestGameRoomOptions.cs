using CommandLine;

namespace TestConsoleApp.CommandLine
{
    [Verb("gameroom", HelpText = "Test GameRoom")]
    public class TestGameRoomOptions:TestOptionsBase
    {
        [Option('c', "channel-id", Default = 1001, Required = true, HelpText = "Set ChannelId")]
        public int ChannelId { get; set; }

        [Option('h', "channel-users", Default = 400, Required = false, HelpText = "Set channel user count.")]
        public int ChannelUsers { get; set; }

        [Option('o', "room-users", Default = 5, Required = false, HelpText = "Set user count.")]
        public int RoomUsers { get; set; }

        [Option('r', "rooms", Default = 20, Required = false, HelpText = "Set room count.")]
        public int Rooms { get; set; }
    }
}