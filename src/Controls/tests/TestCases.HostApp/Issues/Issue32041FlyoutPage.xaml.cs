#if ANDROID
using Android.Views;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32041, "FlyoutPage - Keyboard overlaps when SoftInput.AdjustResize is set", PlatformAffected.Android, issueTestNumber: 4)]
public partial class Issue32041FlyoutPage : FlyoutPage
{
	public Issue32041FlyoutPage()
	{
		InitializeComponent();

#if ANDROID
		// Set SoftInput.AdjustResize - the entire FlyoutPage container should resize
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(SoftInput.AdjustResize | SoftInput.StateUnspecified);
#endif
	}

	private void OnToggleFlyout(object sender, EventArgs e)
	{
		IsPresented = !IsPresented;
	}
}
