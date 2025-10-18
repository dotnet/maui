#if WINDOWS
using Microsoft.Web.WebView2.Core;
#elif IOS || MACCATALYST
using WebKit;
#elif ANDROID
using Android.Webkit;
#endif

using Microsoft.Maui;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests;

public class PlatformWebViewInitializedEventArgsTests
{
#if !IOS && !MACCATALYST && !ANDROID && !WINDOWS

    /// <summary>
    /// Tests that the PlatformWebViewInitializedEventArgs constructor can be instantiated with a valid WebViewInitializationCompletedEventArgs instance.
    /// This test verifies the constructor accepts valid input and creates the instance successfully.
    /// Expected result: Constructor completes without throwing an exception.
    /// </summary>
    [Fact]
    public void Constructor_WithValidArgs_CreatesInstance()
    {
        // Arrange
        var args = new WebViewInitializationCompletedEventArgs();

        // Act & Assert
        var result = new PlatformWebViewInitializedEventArgs(args);

        Assert.NotNull(result);
    }

    /// <summary>
    /// Tests that the PlatformWebViewInitializedEventArgs constructor can handle null input.
    /// This test verifies the constructor's behavior when passed a null argument.
    /// Expected result: Constructor completes without throwing an exception since the parameter is not used.
    /// </summary>
    [Fact]
    public void Constructor_WithNullArgs_CreatesInstance()
    {
        // Arrange
        WebViewInitializationCompletedEventArgs args = null;

        // Act & Assert
        var result = new PlatformWebViewInitializedEventArgs(args);

        Assert.NotNull(result);
    }

#endif
}
