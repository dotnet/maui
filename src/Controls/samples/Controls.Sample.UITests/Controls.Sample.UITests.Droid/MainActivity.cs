using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Microsoft.Maui;

namespace Maui.Controls.Sample.Droid
{
	[Activity(
		Theme = "@style/Maui.SplashTheme", 
		MainLauncher = true,
		LaunchMode = LaunchMode.SingleTask,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
	[IntentFilter(
		new[] { Microsoft.Maui.ApplicationModel.Platform.Intent.ActionAppAction },
		Categories = new[] { Android.Content.Intent.CategoryDefault })]
	[Register("com.microsoft.maui.uitests.MainActivity")]
	public class MainActivity : MauiAppCompatActivity
	{
	}
}