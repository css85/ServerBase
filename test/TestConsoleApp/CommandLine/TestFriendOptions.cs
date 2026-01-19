using CommandLine;

namespace TestConsoleApp.CommandLine
{
    [Verb("friend", HelpText = "Test Friend")]
    public class TestFriendOptions:TestOptionsBase
    {
        [Option('s', "startid", Default = 1, Required = true, HelpText = "Set Start ID.")]
        public int StartID { get; set; }

        [Option('r', "createuser", Default = true, Required = true, HelpText = "Is Create User")]
        public bool IsCreateUser { get; set; }

        [Option('v', "validcheck", Default = true, Required = true, HelpText = "Valid Check")]
        public bool IsValidCheck { get; set; }
    }
}