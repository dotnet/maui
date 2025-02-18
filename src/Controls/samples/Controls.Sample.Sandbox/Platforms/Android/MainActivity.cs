using Android.App;
using Android.Content.PM;
using Microsoft.Maui;

namespace Maui.Controls.Sample.Platform
{
	[Activity(
		Theme = "@style/Maui.SplashTheme",
		MainLauncher = true,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode)]
	[IntentFilter(
		new[] { Microsoft.Maui.ApplicationModel.Platform.Intent.ActionAppAction },
		Categories = new[] { Android.Content.Intent.CategoryDefault })]
	public class MainActivity : MauiAppCompatActivity
	{
		public override bool OnKeyUp([Android.Runtime.GeneratedEnum] Android.Views.Keycode keyCode, Android.Views.KeyEvent e)
		{
			/*
			bool wasHandled = PlatformService.HandleOnKeyUp(keyCode, e);
			if (!wasHandled)
				return base.OnKeyUp(keyCode, e);
			else
				return true;
			*/
			Console.WriteLine($"OnKeyUp {keyCode}, {e.Action} {e.RepeatCount}");

			return base.OnKeyUp(keyCode, e);
		}
	}
}