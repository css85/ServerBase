using CommandLine;

namespace TestConsoleApp.CommandLine
{
    [Verb("chat", HelpText = "Test Chat")]
    public class TestChatOptions:TestOptionsBase
    {
        [Option("startid", Default = 1, Required = true, HelpText = "Set Start ID.")]
        public int StartID { get; set; }

        [Option("createuser", Default = true, Required = true, HelpText = "Is Create User")]
        public bool IsCreateUser { get; set; }

        [Option("worldchat", Default = 0, Required = true, HelpText = "Is World Chat")]
        public int IsWorldChat { get; set; }

        [Option("validcheck", Default = true, Required = true, HelpText = "Valid Check")]
        public bool IsValidCheck { get; set; }

        [Option("sendchatroommessagecount", Default = 10, Required = true, HelpText = "Send Chat Room Message Count")]
        public int SendCharRoomMessageCount { get; set; }

        [Option("sendworldchatmessagecount", Default = 10, Required = true, HelpText = "Send World Chat Message Count")]
        public int SendWorldChatMessageCount { get; set; }

        [Option("createchatroomrandomvalue", Default = 20, Required = true, HelpText = "Create Chat Room Random Value")]
        public int CreatechatRoomRandomValue { get; set; }

        [Option("invitechatroomrandomvalue", Default = 5, Required = true, HelpText = "Invite Chat Room Random Value")]
        public int InviteChatRoomRandomValue { get; set; }

        [Option("leavechatroomrandomvalue", Default = 5, Required = true, HelpText = "Leave Chat Room Random Value")]
        public int LeaveChatRoomRandomValue { get; set; }
    }
}