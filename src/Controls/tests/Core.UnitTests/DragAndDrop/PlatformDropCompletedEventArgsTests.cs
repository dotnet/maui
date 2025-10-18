using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests;

public partial class PlatformDropCompletedEventArgsTests
{
#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
    /// <summary>
    /// Tests that the parameterless constructor successfully creates an instance
    /// when no platform-specific symbols are defined.
    /// </summary>
    [Fact]
    public void Constructor_Default_CreatesInstance()
    {
        // Arrange & Act
        var result = new PlatformDropCompletedEventArgs();

        // Assert
        Assert.NotNull(result);
    }
#endif
}
