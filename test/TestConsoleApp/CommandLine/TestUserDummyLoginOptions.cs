using CommandLine;

namespace TestConsoleApp.CommandLine
{
    [Verb("dummy-user-login", HelpText = "dummy User Auth Login")]
    public class TestUserDummyLoginOptions : TestOptionsBase
    {
        [Option('s', "range-start", Default = 0, Required = false, HelpText = "Set dummy range start.")]
        public long RangeStartUserSeq { get; set; }
        
        [Option('e', "range-end", Default = 0, Required = false, HelpText = "Set dummy range end.")]
        public long RangeEndUserSeq { get; set; }
        
        [Option('a', "prefix-account", Default = "test_account_", Required = false, HelpText = "Set prefix account count.")]
        public string PrefixAccount { get; set; }
    }
}