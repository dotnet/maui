namespace Microsoft.Maui.Cli.UnitTests.Commands.Screenshot;

public class ScreenshotCommandTests
{
    [Fact]
    public void Screenshot_Help_ShowsDescription()
    {
        var output = CliTestHelper.RunCli("screenshot", "--help");

        Assert.Contains("Captures a screenshot of the currently running .NET MAUI application", output);
    }

    [Fact]
    public void Screenshot_Help_ShowsOutputOption()
    {
        var output = CliTestHelper.RunCli("screenshot", "--help");

        Assert.Contains("--output", output);
        Assert.Contains("Output file path", output);
    }

    [Fact]
    public void Screenshot_Help_ShowsWaitOption()
    {
        var output = CliTestHelper.RunCli("screenshot", "--help");

        Assert.Contains("--wait", output);
        Assert.Contains("Wait time in seconds", output);
    }

    [Fact]
    public void Screenshot_Help_ShowsGlobalOptions()
    {
        var output = CliTestHelper.RunCli("screenshot", "--help");

        // Global options should be available on subcommands
        Assert.Contains("--framework", output);
        Assert.Contains("--device", output);
        Assert.Contains("--project", output);
    }

    [Fact]
    public void Screenshot_WithoutFramework_UsesCurrentPlatform()
    {
        var output = CliTestHelper.RunCli("screenshot", "-o", "test.png");

        // Should show capturing message for current platform (not an error)
        Assert.Contains("Capturing", output);
    }

    [Fact]
    public void Screenshot_WithAndroidFramework_ShowsCapturingMessage()
    {
        var output = CliTestHelper.RunCli("screenshot", "-f", "net10.0-android", "-o", "test.png");

        Assert.Contains("Capturing Android screenshot", output);
    }

    [Fact]
    public void Screenshot_WithiOSFramework_ShowsCapturingMessage()
    {
        var output = CliTestHelper.RunCli("screenshot", "-f", "net10.0-ios", "-o", "test.png");

        Assert.Contains("Capturing iOS Simulator screenshot", output);
    }

    [Fact]
    public void Screenshot_WithMacCatalystFramework_ShowsCapturingMessage()
    {
        var output = CliTestHelper.RunCli("screenshot", "-f", "net10.0-maccatalyst", "-o", "test.png");

        Assert.Contains("Capturing Mac Catalyst screenshot", output);
    }

    [Fact]
    public void Screenshot_WithWindowsFramework_ShowsCapturingMessage()
    {
        var output = CliTestHelper.RunCli("screenshot", "-f", "net10.0-windows", "-o", "test.png");

        Assert.Contains("Capturing Windows screenshot", output);
    }
}
