using CommandLine;

namespace TestConsoleApp.CommandLine
{
    [Verb("world-chat-stress-text", HelpText = "Test WorldChatStressWithTextMessage")]
    public class TestWorldChatStressWithTextMessageOptions:TestOptionsBase
    {
        [Option('t', "chat-host", Required = true, HelpText = "Set Chat host")]
        public string ChatHost { get; set; }

        [Option('r', "random-chat-message", Default = true, Required = false, HelpText = "Is random chat message")]
        public bool RandomChatMessage { get; set; }

        [Option('m', "chat-message", Default = "", Required = false, HelpText = "Set chat message.")]
        public string ChatMessage { get; set; }

        [Option('d', "chat-delay", Default = 10, Required = false, HelpText = "Set chat delay time milliseconds.")]
        public int ChatDelayTimeMilliseconds { get; set; }

        [Option('c', "user-chat-delay", Default = 10, Required = false, HelpText = "Set chat delay time milliseconds.")]
        public int UserChatDelayTimeMilliseconds { get; set; }
    }
}