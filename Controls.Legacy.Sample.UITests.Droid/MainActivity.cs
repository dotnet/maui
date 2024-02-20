using Android.App;
using Android.Content.PM;

namespace Maui.Controls.Legacy.Sample.Droid;

[Activity(
	Theme = "@style/Maui.SplashTheme", 
	MainLauncher = true, 
	ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize)]
public class MainActivity : MauiAppCompatActivity
{
}
