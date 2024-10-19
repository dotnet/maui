using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using AndroidX.Lifecycle;

namespace Maui.Controls.Sample.Platform
{
	[Activity(
		Theme = "@style/Maui.SplashTheme",
		MainLauncher = true,
		LaunchMode = LaunchMode.SingleTask,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode)]
	[IntentFilter(
		new[] { Microsoft.Maui.ApplicationModel.Platform.Intent.ActionAppAction },
		Categories = new[] { Android.Content.Intent.CategoryDefault })]
	[Register("com.microsoft.maui.uitests.MainActivity")]
	public class MainActivity : MauiAppCompatActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			Lifecycle.AddObserver(new Issue14037LifecycleObserver(ActivityResultRegistry));
			base.OnCreate(savedInstanceState);
		}
	}
}