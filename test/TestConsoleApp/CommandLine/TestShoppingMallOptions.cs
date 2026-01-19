using CommandLine;

namespace TestConsoleApp.CommandLine
{
    [Verb("shoppingmall", HelpText = "Test ShoppingMall")]
    public class TestShoppingMallOptions:TestOptionsBase
    {
        [Option('s', "startid", Default = 1, Required = true, HelpText = "Set Start ID.")]
        public int StartID { get; set; }

        [Option('r', "createuser", Default = true, Required = true, HelpText = "Is Create User")]
        public bool IsCreateUser { get; set; }

        [Option('G', "gradecheck", Default = true, Required = true, HelpText = "Is Grade Check")]
        public bool IsGradeCheck { get; set; }

        [Option('p', "purchasecount", Default = 10, Required = true, HelpText = "Purchase Count")]
        public int PurchaseCount { get; set; }
    }
}