using Microsoft.Maui;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests;

public class PlatformDragStartingEventArgsTests
{
#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
    /// <summary>
    /// Tests that the parameterless constructor successfully creates an instance.
    /// This test verifies the fallback constructor used on platforms other than iOS, macCatalyst, Android, or Windows.
    /// Expected result: A non-null instance is created.
    /// </summary>
    [Fact]
    public void Constructor_DefaultPlatform_CreatesInstance()
    {
        // Arrange & Act
        var instance = new PlatformDragStartingEventArgs();

        // Assert
        Assert.NotNull(instance);
    }
#endif
}
