using CommandLine;

namespace TestConsoleApp.CommandLine
{
    public abstract class TestOptionsBase : ITestOptions
    {
        [Option("host", Required = true, HelpText = "Set Api host")]
        public string ApiHost { set; get; }
        
        [Option("users", Default = 100, Required = false, HelpText = "Set user count.")]
        public int UserCount { get; set; }
        
        [Option("batch-count", Default = 500, Required = false, HelpText = "batch count.")]
        public int BatchProcessingCount { get; set; }
        
        [Option("batch-interval", Default = 100, Required = false, HelpText = "processing batch interval.")]
        public int BatchIntervalMilliseconds { get; set; }
        
        [Option("cycle-delay", Default = 1000, Required = false, HelpText = "Set cycle delay time seconds.")]
        public int CycleDelayMilliseconds { get; set; }
    }
}