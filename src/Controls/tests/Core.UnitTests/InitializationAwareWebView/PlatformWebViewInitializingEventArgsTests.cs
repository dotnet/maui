#if IOS || MACCATALYST
using WebKit;
#elif ANDROID
using Android.Webkit;
#elif WINDOWS
using Microsoft.Web.WebView2.Core;
#endif

using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Xunit;


namespace Microsoft.Maui.Controls.Core.UnitTests;

/// <summary>
/// Unit tests for PlatformWebViewInitializingEventArgs class.
/// </summary>
public class PlatformWebViewInitializingEventArgsTests
{
    /// <summary>
    /// Tests that the constructor accepts a valid WebViewInitializationStartedEventArgs parameter
    /// and correctly assigns it to the internal field.
    /// </summary>
    [Fact]
    public void Constructor_ValidParameter_AssignsParameterToField()
    {
        // Arrange
#if WINDOWS || (!IOS && !MACCATALYST && !ANDROID)
        var args = new WebViewInitializationStartedEventArgs();
#elif IOS || MACCATALYST
            var configuration = new WebKit.WKWebViewConfiguration();
            var args = new WebViewInitializationStartedEventArgs(configuration);
#elif ANDROID
            var settings = new Android.Webkit.WebSettings(null);
            var args = new WebViewInitializationStartedEventArgs(settings);
#endif

        // Act
        var platformArgs = new PlatformWebViewInitializingEventArgs(args);

        // Assert
        // We need to access the private field through reflection since it's readonly and internal
        var field = typeof(PlatformWebViewInitializingEventArgs).GetField("_coreArgs",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var fieldValue = field.GetValue(platformArgs);

        Assert.Same(args, fieldValue);
    }

    /// <summary>
    /// Tests that the constructor accepts a null parameter and stores it without throwing an exception.
    /// The constructor does not perform null validation.
    /// </summary>
    [Fact]
    public void Constructor_NullParameter_StoresNullValue()
    {
        // Arrange
        WebViewInitializationStartedEventArgs args = null;

        // Act
        var platformArgs = new PlatformWebViewInitializingEventArgs(args);

        // Assert
        // We need to access the private field through reflection since it's readonly and internal
        var field = typeof(PlatformWebViewInitializingEventArgs).GetField("_coreArgs",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var fieldValue = field.GetValue(platformArgs);

        Assert.Null(fieldValue);
    }

    /// <summary>
    /// Tests that the constructor properly initializes the instance and the field is accessible.
    /// </summary>
    [Fact]
    public void Constructor_ValidParameter_CreatesValidInstance()
    {
        // Arrange
#if WINDOWS || (!IOS && !MACCATALYST && !ANDROID)
        var args = new WebViewInitializationStartedEventArgs();
#elif IOS || MACCATALYST
            var configuration = new WebKit.WKWebViewConfiguration();
            var args = new WebViewInitializationStartedEventArgs(configuration);
#elif ANDROID
            var settings = new Android.Webkit.WebSettings(null);
            var args = new WebViewInitializationStartedEventArgs(settings);
#endif

        // Act
        var platformArgs = new PlatformWebViewInitializingEventArgs(args);

        // Assert
        Assert.NotNull(platformArgs);
    }
}
