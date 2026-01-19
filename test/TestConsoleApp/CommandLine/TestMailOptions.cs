using CommandLine;

namespace TestConsoleApp.CommandLine
{
    [Verb("mail", HelpText = "Test Mail")]
    public class TestMailOptions : TestOptionsBase
    {
        [Option("enable-deleteMail", Default= false, HelpText = "Enable Delete Mail Scenario")]
        public bool EnableDeleteMailScenario { get; set; }

    }
}