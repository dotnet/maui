using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Widget;
using Xunit.Runners.UI;
using System.Reflection;
using Android.Content.PM;

namespace Xamarin.Platform.Handlers.DeviceTests
{
    [Activity(
		Name = "com.xamarin.handlers.devicetests.MainActivity",
		Label = "@string/app_name",
		Theme = "@style/MainTheme",
		MainLauncher = true,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : RunnerActivity
	{
        protected override void OnCreate(Bundle bundle)
		{
			Xamarin.Essentials.Platform.Init(this, bundle);
			Xamarin.Platform.Handlers.DeviceTests.Platform.Init(this);

			// tests can be inside the main assembly
			AddTestAssembly(Assembly.GetExecutingAssembly());
			AddTestAssembly(typeof(Xamarin.Platform.Handlers.DeviceTests.SliderHandlerTests).Assembly);
			
			base.OnCreate(bundle);
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}
	}
}