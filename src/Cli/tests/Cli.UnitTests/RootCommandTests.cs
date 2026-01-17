namespace Microsoft.Maui.Cli.UnitTests;

public class RootCommandTests
{
    [Fact]
    public void Help_ShowsDescription()
    {
        var output = CliTestHelper.RunCli("--help");

        Assert.Contains("dotnet-maui CLI - Tools for capturing screenshots, viewing logs, and inspecting .NET MAUI applications", output);
    }

    [Fact]
    public void Help_ShowsScreenshotCommand()
    {
        var output = CliTestHelper.RunCli("--help");

        Assert.Contains("screenshot", output);
        Assert.Contains("Captures a screenshot", output);
    }

    [Fact]
    public void Help_ShowsGlobalOptions()
    {
        var output = CliTestHelper.RunCli("--help");

        Assert.Contains("-f, --framework", output);
        Assert.Contains("-d, --device", output);
        Assert.Contains("-p, --project", output);
        Assert.Contains("--verbose", output);
    }

    [Fact]
    public void Version_ShowsVersion()
    {
        var output = CliTestHelper.RunCli("--version");

        // Should output some version string
        Assert.False(string.IsNullOrWhiteSpace(output));
    }

    [Fact]
    public void InvalidCommand_ReturnsNonZeroExitCode()
    {
        var (_, exitCode) = CliTestHelper.RunCliWithExitCode("--invalid-option");

        Assert.NotEqual(0, exitCode);
    }
}
