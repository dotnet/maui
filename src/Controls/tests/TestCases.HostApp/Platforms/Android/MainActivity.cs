using Android.App;
using Android.Content.PM;
using Android.Runtime;
using Microsoft.Maui;

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
		internal static bool UseShellRenderer { get; private set; }

		protected override void OnCreate(Android.OS.Bundle savedInstanceState)
		{
			UseShellRenderer = string.Equals(Intent?.GetStringExtra("TEST_CONFIGURATION_ARGS"), "ShellRenderer", StringComparison.OrdinalIgnoreCase);
			base.OnCreate(savedInstanceState);
		}
	}
}