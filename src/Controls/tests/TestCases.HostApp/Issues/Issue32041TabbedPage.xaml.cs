using Android.Views;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32041, "TabbedPage - Keyboard overlaps when SoftInput.AdjustResize is set", PlatformAffected.Android, issueTestNumber: 3)]
public partial class Issue32041TabbedPage : TabbedPage
{
	public Issue32041TabbedPage()
	{
		InitializeComponent();

#if ANDROID
		// Set SoftInput.AdjustResize - the entire TabbedPage container (including tabs) should resize
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(SoftInput.AdjustResize | SoftInput.StateUnspecified);
#endif
	}
}
