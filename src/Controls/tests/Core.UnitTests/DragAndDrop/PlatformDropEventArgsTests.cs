using Microsoft.Maui;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests;

public class PlatformDropEventArgsTests
{
#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
    /// <summary>
    /// Tests that the default parameterless constructor can be successfully instantiated.
    /// This test validates the fallback constructor behavior when no platform-specific 
    /// preprocessor directives are defined.
    /// </summary>
    [Fact]
    public void Constructor_Default_CreatesValidInstance()
    {
        // Act
        var result = new PlatformDropEventArgs();

        // Assert
        Assert.NotNull(result);
    }
#endif
}
