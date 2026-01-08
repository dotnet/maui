namespace Microsoft.Maui.Cli.UnitTests;

/// <summary>
/// Helper to run CLI commands in tests, synchronized to avoid Console redirection conflicts.
/// </summary>
internal static class CliTestHelper
{
    private static readonly object ConsoleLock = new();

    public static string RunCli(params string[] args)
    {
        var (output, _) = RunCliWithExitCode(args);
        return output;
    }

    public static (string output, int exitCode) RunCliWithExitCode(params string[] args)
    {
        lock (ConsoleLock)
        {
            var originalOut = Console.Out;
            var originalError = Console.Error;

            try
            {
                var sw = new StringWriter();
                Console.SetOut(sw);
                Console.SetError(sw);

                var exitCode = Program.Main(args);

                sw.Flush();
                return (sw.ToString(), exitCode);
            }
            finally
            {
                Console.SetOut(originalOut);
                Console.SetError(originalError);
            }
        }
    }
}
