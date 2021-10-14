using Android.App;
using Android.Content.PM;
using Android.OS;
using Microsoft.Maui;

namespace Maui.Controls.Sample.Droid
{
	[Activity(
		Label = "@string/app_name",
		Theme = "@style/Maui.SplashTheme",
		MainLauncher = true,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.ScreenLayout | ConfigChanges.UiMode)]
	[IntentFilter(
		new[] { Microsoft.Maui.Essentials.Platform.Intent.ActionAppAction },
		Categories = new[] { Android.Content.Intent.CategoryDefault })]
	public class MainActivity : MauiAppCompatActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			Android.Util.Log.Debug("JWM", "Activity.OnCreate - DualScreenService.Init");
			Microsoft.Maui.Controls.DualScreen.DualScreenService.Init(this);
			base.OnCreate(savedInstanceState);
		}
	}
}