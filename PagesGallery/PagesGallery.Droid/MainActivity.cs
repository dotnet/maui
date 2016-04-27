using Android.App;
using Android.Content.PM;
using Android.OS;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;

namespace PagesGallery.Droid
{
	[Activity(Label = "PagesGallery", Theme = "@style/MyTheme", Icon = "@drawable/icon", MainLauncher = true,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : FormsAppCompatActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			ToolbarResource = Resource.Layout.Toolbar;
			TabLayoutResource = Resource.Layout.Tabbar;

			Microsoft.WindowsAzure.MobileServices.CurrentPlatform.Init();

			base.OnCreate(bundle);
			
			Forms.Init(this, bundle);
			LoadApplication(new App());
		}
	}
}