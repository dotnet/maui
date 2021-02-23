using Android.App;
using Android.OS;
using Android.Views;
using Xamarin.Platform;

namespace Maui.Controls.Sample.Droid
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
	public class MainActivity : MauiAppCompatActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			//Xamarin.Essentials.Platform.Init(this, savedInstanceState);
		}
	}
}