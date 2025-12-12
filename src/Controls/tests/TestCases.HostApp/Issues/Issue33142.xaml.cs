#if ANDROID
using Android.Views;
#endif

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 33142, "Keyboard overlaps Entry with SafeAreaEdges.All", PlatformAffected.Android)]
public partial class Issue33142 : ContentPage
{
	public Issue33142()
	{
		InitializeComponent();

#if ANDROID
		// Set SoftInput.AdjustNothing - SafeAreaEdges.All on Grid should handle keyboard insets
		var window = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity?.Window;
		window?.SetSoftInputMode(SoftInput.AdjustNothing | SoftInput.StateUnspecified);
#endif
	}
}
