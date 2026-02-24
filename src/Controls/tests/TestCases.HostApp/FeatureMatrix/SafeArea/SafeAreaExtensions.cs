namespace Maui.Controls.Sample;

/// <summary>
/// Local safe area extension methods for test pages.
/// Mirrors the framework's internal SafeAreaPadding/SafeAreaExtensions logic
/// using the same public platform APIs.
/// </summary>
static class SafeAreaExtensions
{
    /// <summary>
    /// Gets the raw safe area insets (system bars + display cutout) as a formatted string.
    /// Format: "L:{left},T:{top},R:{right},B:{bottom}, KeyboardHeight:{keyboardHeight}" (in pixels).
    /// </summary>
    public static string GetSafeAreaInfo(Page page)
    {
#if ANDROID
        try
        {
            if (Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window?.DecorView is Android.Views.View decorView)
            {
                var insets = AndroidX.Core.View.ViewCompat.GetRootWindowInsets(decorView);
                if (insets != null)
                {
                    var systemBars = insets.GetInsets(AndroidX.Core.View.WindowInsetsCompat.Type.SystemBars());
                    var ime = insets.GetInsets(AndroidX.Core.View.WindowInsetsCompat.Type.Ime());
                    return $"L:{systemBars.Left},T:{systemBars.Top},R:{systemBars.Right},B:{systemBars.Bottom},KeyboardHeight:{ime.Bottom}";
                }
            }
        }
        catch { }
#elif IOS || MACCATALYST
        try
        {
            if (page.Handler?.PlatformView is UIKit.UIView platformView && platformView.Window != null)
            {
                var safeAreaInsets = platformView.Window.SafeAreaInsets;
                return $"L:{(int)safeAreaInsets.Left},T:{(int)safeAreaInsets.Top},R:{(int)safeAreaInsets.Right},B:{(int)safeAreaInsets.Bottom},KeyboardHeight:0";
            }
        }
        catch { }
#endif
        return "L:0,T:0,R:0,B:0,KeyboardHeight:0";
    }
}
