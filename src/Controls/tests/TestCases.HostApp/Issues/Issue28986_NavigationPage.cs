#if ANDROID
using Android.Views;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 28986, "Test SafeArea Navigation Page for per-edge safe area control", PlatformAffected.Android | PlatformAffected.iOS, issueTestNumber: 7)]
public partial class Issue28986_NavigationPage : NavigationPage
{
	public Issue28986_NavigationPage() : base(new Issue28986_ContentPage())
	{
        BarBackground = Colors.Blue;

#if ANDROID
		// Set SoftInput.AdjustNothing - we have full control over insets (iOS-like behavior)
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(SoftInput.AdjustNothing | SoftInput.StateUnspecified);
#endif
	}
}