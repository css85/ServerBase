using CommandLine;

namespace TestConsoleApp.CommandLine
{
    [Verb("showdancer", HelpText = "Test ShowDancer")]
    public class TestShowDancerOptions:TestOptionsBase
    {
        [Option('s', "startid", Default = 1, Required = true, HelpText = "Set Start ID.")]
        public int StartID { get; set; }

        [Option('r', "createuser", Default = true, Required = true, HelpText = "Is Create User")]
        public bool IsCreateUser { get; set; }

        [Option('p', "purchasecount", Default = 100, Required = false, HelpText = "Purchase ShowDancer Item")]
        public int PurchaseCount { get; set; }
    }
}