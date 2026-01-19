namespace SampleGame.Shared.Extensions
{
    public static class ArgsExtensions
    {
        public static string FindArgValue(this string[] args, string argName)
        {
            if (args == null)
                return "";
            if (args.Length < 2)
                return "";

            for (var i = 0; i < args.Length; i++)
            {
                if (args[i] == argName)
                {
                    if (args.Length <= i + 1)
                        return "";

                    return args[i + 1];
                }
            }

            return "";
        }
    }
}