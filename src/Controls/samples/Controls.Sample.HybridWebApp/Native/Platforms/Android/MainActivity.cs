using Android.App;
using Android.Content.PM;
using Microsoft.Maui;

namespace Maui.Controls.Sample.HybridWebApp;

[Activity(Theme = "@style/Maui.MainTheme.NoActionBar", MainLauncher = true, LaunchMode = LaunchMode.SingleTop,
	ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode |
		ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
public class MainActivity : MauiAppCompatActivity
{
}
