using Android.App;
using Microsoft.Maui;

namespace Maui.Controls.Sample.Droid
{
	[Activity(Label = "@string/app_name", Theme = "@style/Maui.SplashTheme", MainLauncher = true)]
	[IntentFilter(
		new[] { Microsoft.Maui.Essentials.Platform.Intent.ActionAppAction },
		Categories = new[] { Android.Content.Intent.CategoryDefault })]
	public class MainActivity : MauiAppCompatActivity
	{
	}
}