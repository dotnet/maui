#if ANDROID
using Android.Views;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32041, "Keyboard overlaps Entry when SoftInput.AdjustResize is set", PlatformAffected.Android)]
public partial class Issue32041 : ContentPage
{
	public Issue32041()
	{
		InitializeComponent();

#if ANDROID
		// Set SoftInput.AdjustResize to simulate the user's scenario
		// This should cause the content to be resized (not overlapped) when the keyboard appears
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(SoftInput.AdjustResize | SoftInput.StateUnspecified);
#endif
	}
}
