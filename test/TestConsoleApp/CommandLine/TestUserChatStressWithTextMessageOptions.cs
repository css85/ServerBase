using CommandLine;

namespace TestConsoleApp.CommandLine
{
    [Verb("user-chat-stress-text", HelpText = "Test UserChatStressWithTextMessage")]
    public class TestUserChatStressWithTextMessageOptions:TestOptionsBase
    {
        [Option('t', "chat-host", Required = true, HelpText = "Set Chat host")]
        public string ChatHost { get; set; }

        [Option('r', "random-chat-message", Default = true, Required = false, HelpText = "Is random chat message")]
        public bool RandomChatMessage { get; set; }

        [Option('m', "chat-message", Default = "", Required = false, HelpText = "Set chat message.")]
        public string ChatMessage { get; set; }

        [Option('u', "room-count", Default = 500, Required = false, HelpText = "Set room count.")]
        public int RoomCount { get; set; }

        [Option('o', "room-user-count", Default = 5, Required = false, HelpText = "Set room user count.")]
        public int RoomUserCount { get; set; }

        [Option('c', "chat-repeat-count", Default = 5, Required = false, HelpText = "Set chat repeat count.")]
        public int ChatRepeatCount { get; set; }

        [Option('d', "chat-delay", Default = 1000, Required = false, HelpText = "Set chat delay time milliseconds.")]
        public int ChatDelayTimeMilliseconds { get; set; }
    }
}