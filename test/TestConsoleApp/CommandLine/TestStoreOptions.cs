using CommandLine;

namespace TestConsoleApp.CommandLine
{
    [Verb("store", HelpText = "Test Store")]
    public class TestStoreOptions :TestOptionsBase
    {
        [Option('s', "startid", Default = 1, Required = false, HelpText = "Set Start ID.")]
        public int StartID { get; set; }

        [Option('r', "createuser", Default = true, Required = false, HelpText = "Is Create User")]
        public bool IsCreateUser { get; set; }
        
        
        [Option("giftTake", Default = 1, Required = false, HelpText = "인앱 아이템 개수")]
        public int GiftTakeCount { get; set; }
        
        [Option("loopCount", Default = 1, Required = false, HelpText = "인앱 아이템 반복")]
        public int LoopCount { get; set; }
    }
}