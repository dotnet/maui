#if ANDROID
using Android.Views;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33142, "FlyoutPage - Keyboard handling with SafeAreaEdges.All", PlatformAffected.Android, issueTestNumber: 4)]
public partial class Issue33142FlyoutPage : FlyoutPage
{
	public Issue33142FlyoutPage()
	{
		InitializeComponent();

#if ANDROID
		// Set SoftInput.AdjustNothing - SafeAreaEdges.All on Grid should handle keyboard insets
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(SoftInput.AdjustNothing | SoftInput.StateUnspecified);
#endif
	}

	private void OnToggleFlyout(object sender, EventArgs e)
	{
		IsPresented = !IsPresented;
	}
}
