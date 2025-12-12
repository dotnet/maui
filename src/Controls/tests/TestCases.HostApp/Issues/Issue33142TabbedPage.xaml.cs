#if ANDROID
using Android.Views;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33142, "TabbedPage - Keyboard handling with SafeAreaEdges.All", PlatformAffected.Android, issueTestNumber: 3)]
public partial class Issue33142TabbedPage : TabbedPage
{
	public Issue33142TabbedPage()
	{
		InitializeComponent();

#if ANDROID
		// Set SoftInput.AdjustNothing - SafeAreaEdges.All on Grid should handle keyboard insets
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(SoftInput.AdjustNothing | SoftInput.StateUnspecified);
#endif
	}
}
