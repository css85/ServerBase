using CommandLine;

namespace TestConsoleApp.CommandLine
{
    [Verb("couple", HelpText = "Test Couple")]
    public class TestCoupleOptions : TestOptionsBase
    {
        [Option("startid", Default = 1, Required = true, HelpText = "Set Start ID.")]
        public int StartID { get; set; }

        [Option("createuser", Default = true, Required = true, HelpText = "Is Create User")]
        public bool IsCreateUser { get; set; }

        [Option("removecouplerequestrandomvalue", Default = 5, Required = true, HelpText = "Remove Couple Request Random Value")]
        public int RemoveCoupleRequestRandomValue { get; set; }

        [Option("couplerequestrandomvalue", Default = 5, Required = true, HelpText = "Couple Request Random Value")]
        public int CoupleRequestRandomValue { get; set; }

        [Option("coupleacceptrandomvalue", Default = 5, Required = true, HelpText = "Couple Accept Random Value")]
        public int CoupleAcceptRandomValue { get; set; }

        [Option("couplebreakrandomvalue", Default = 5, Required = true, HelpText = "Couple Break Random Value")]
        public int CoupleBreakRandomValue { get; set; }
    }
}