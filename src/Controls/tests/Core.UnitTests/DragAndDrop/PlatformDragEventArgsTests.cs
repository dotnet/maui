using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests;

/// <summary>
/// Unit tests for PlatformDragEventArgs class.
/// </summary>
public class PlatformDragEventArgsTests
{
#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS
    /// <summary>
    /// Tests that the parameterless constructor creates a valid instance.
    /// Verifies that the default constructor can be called without throwing exceptions
    /// and creates a non-null instance on platforms other than iOS, macCatalyst, Android, and Windows.
    /// </summary>
    [Fact]
    public void Constructor_Default_CreatesValidInstance()
    {
        // Arrange & Act
        var platformDragEventArgs = new PlatformDragEventArgs();

        // Assert
        Assert.NotNull(platformDragEventArgs);
    }
#endif
}
