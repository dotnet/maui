using Android.Views;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 32041, "Shell with bottom tabs - Keyboard overlaps when SoftInput.AdjustResize is set", PlatformAffected.Android, issueTestNumber: 5)]
public partial class Issue32041Shell : Shell
{
	public Issue32041Shell()
	{
		InitializeComponent();

#if ANDROID
		// Set SoftInput.AdjustResize - the entire Shell container (including bottom tabs) should resize
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(SoftInput.AdjustResize | SoftInput.StateUnspecified);
#endif
	}
}
