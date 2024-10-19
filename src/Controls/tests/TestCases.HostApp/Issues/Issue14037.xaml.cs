#if ANDROID
using Maui.Controls.Sample.Platform;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 14037, "[Android] MauiAppCompatActivity default prevents getting result from an Activity.", PlatformAffected.Android, NavigationBehavior.PushAsync)]
public partial class Issue14037 : ContentPage
{
	// Pre-conditions:
	//	1. Before running this test, make sure the test device has the dev setting "Don't keep activities" enabled.
	//		- It may be possible to do: "adb shell settings put global always_finish_activities 1" but I don't think UI tests have this capability...
	//			- It may also not work as intended (https://stackoverflow.com/questions/64076039/adb-command-toggles-dont-keep-activities-developer-options-setting-but-it-has-n).
	public Issue14037()
	{
		InitializeComponent();
	}

	private async void LaunchActivityForResult_Clicked(object sender, EventArgs e)
	{
		var result = await Issue14037LifecycleObserver.Launch();
		Result.Text = result.ToString();
	}
}
#endif