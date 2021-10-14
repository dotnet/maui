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
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode)]
	[IntentFilter(
		new[] { Microsoft.Maui.Essentials.Platform.Intent.ActionAppAction },
		Categories = new[] { Android.Content.Intent.CategoryDefault })]
	public class MainActivity : MauiAppCompatActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			Microsoft.Maui.Controls.DualScreen.DualScreenService.Init(this);
			base.OnCreate(savedInstanceState);
		}
	}
}