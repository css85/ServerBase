using CommandLine;

namespace TestConsoleApp.CommandLine
{
    [Verb("sponsor", HelpText = "Test SponsorChance")]
    public class TestSponsorChanceOptions : TestOptionsBase
    {
        [Option("player-count", Default = 10, Required = false, HelpText = "PlayerCount")]
        public int PlayerCount { get; set; }

        [Option("support-count", Default = 10, Required = false, HelpText = "SupportCount")]
        public int SupportCount { get; set; }
    }
}