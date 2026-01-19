using CommandLine;

namespace TestConsoleApp.CommandLine
{
    [Verb("fam", HelpText = "Test Fam")]
    public class TestFamOptions:TestOptionsBase
    {
        [Option("startid", Default = 1, Required = true, HelpText = "Set Start ID.")]
        public int StartID { get; set; }

        [Option("createuser", Default = true, Required = true, HelpText = "Is Create User")]
        public bool IsCreateUser { get; set; }

        [Option("validcheck", Default = true, Required = true, HelpText = "Valid Check")]
        public bool IsValidCheck { get; set; }

        [Option("createfamrandomvalue", Default = 20, Required = true, HelpText = "Create Fam Random Value")]
        public int CreateFamRandomValue { get; set; }

        [Option("joinfamrandomvalue", Default = 5, Required = true, HelpText = "Join Fam Random Value")]
        public int JoinFamRandomValue { get; set; }

        [Option("leavefamrandomvalue", Default = 5, Required = true, HelpText = "Leave Fam Random Value")]
        public int LeaveFamRandomValue { get; set; }
    }
}