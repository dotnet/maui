using Microsoft.Maui.Cli.Commands.Screenshot;

namespace Microsoft.Maui.Cli.UnitTests.Commands.Screenshot;

public class GetTargetPlatformIdentifierTests
{
    [Theory]
    [InlineData("net10.0-android", "android")]
    [InlineData("net10.0-ios", "ios")]
    [InlineData("net10.0-maccatalyst", "maccatalyst")]
    [InlineData("net10.0-windows", "windows")]
    [InlineData("net9.0-android", "android")]
    [InlineData("net8.0-ios", "ios")]
    [InlineData("net10.0-android34.0", "android")]
    [InlineData("net10.0-ios18.0", "ios")]
    public void GetTargetPlatformIdentifier_ValidFramework_ReturnsPlatform(string framework, string expectedPlatform)
    {
        var result = ScreenshotCommand.GetTargetPlatformIdentifier(framework);

        Assert.Equal(expectedPlatform, result, ignoreCase: true);
    }

    [Theory]
    [InlineData("net10.0")]
    [InlineData("netstandard2.0")]
    public void GetTargetPlatformIdentifier_FrameworkWithoutPlatform_ReturnsNull(string? framework)
    {
        var result = ScreenshotCommand.GetTargetPlatformIdentifier(framework);

        Assert.Null(result);
    }

    [Fact]
    public void GetTargetPlatformIdentifier_NullOrEmpty_ReturnsCurrentPlatform()
    {
        var resultNull = ScreenshotCommand.GetTargetPlatformIdentifier(null);
        var resultEmpty = ScreenshotCommand.GetTargetPlatformIdentifier("");

        // Should return current OS platform
        Assert.NotNull(resultNull);
        Assert.NotNull(resultEmpty);
        Assert.Equal(resultNull, resultEmpty);

        // Verify it's a valid platform
        var validPlatforms = new[] { "windows", "macos", "maccatalyst", "linux", "ios", "android" };
        Assert.Contains(resultNull, validPlatforms);
    }
}
