#if ANDROID
using Android.Views;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32041, "Verify AdjustPan mode does not apply keyboard insets", PlatformAffected.Android, issueTestNumber: 2)]
public partial class Issue32041AdjustPan : ContentPage
{
	public Issue32041AdjustPan()
	{
		InitializeComponent();

#if ANDROID
		// Set SoftInput.AdjustPan - this should NOT apply insets
		// The window should pan instead
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(SoftInput.AdjustPan | SoftInput.StateUnspecified);
#endif
	}
}
