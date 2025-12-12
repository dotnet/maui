#if ANDROID
using Android.Views;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33142, "Verify AdjustNothing mode does not apply keyboard insets", PlatformAffected.Android, issueTestNumber: 2)]
public partial class Issue33142AdjustPan : ContentPage
{
	public Issue33142AdjustPan()
	{
		InitializeComponent();

#if ANDROID
		// Set SoftInput.AdjustNothing - this should NOT apply insets
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(SoftInput.AdjustNothing | SoftInput.StateUnspecified);
#endif
	}
}
