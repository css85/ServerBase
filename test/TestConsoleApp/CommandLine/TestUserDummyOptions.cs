using CommandLine;

namespace TestConsoleApp.CommandLine
{
    [Verb("dummy-user", HelpText = "Test Make DummyUser")]
    public class TestUserDummyOptions :TestOptionsBase
    {
        [Option('n', "nick-prefix", Default = "test-user", Required = false, HelpText = "Set user nick prefix.")]
        public string NickPrefix { get; set; }
    }
}