using Microsoft.Maui;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests;

/// <summary>
/// Unit tests for the PlatformPointerEventArgs class.
/// </summary>
public partial class PlatformPointerEventArgsTests
{
#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
    /// <summary>
    /// Tests that the parameterless constructor creates a valid instance without throwing exceptions.
    /// This test verifies the fallback constructor behavior on unsupported platforms.
    /// </summary>
    [Fact]
    public void Constructor_DefaultFallback_CreatesValidInstance()
    {
        // Arrange & Act
        var platformArgs = new PlatformPointerEventArgs();

        // Assert
        Assert.NotNull(platformArgs);
    }
#endif
}
