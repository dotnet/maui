using System.Reflection;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Xunit.Runners.UI;

namespace DeviceTests.Droid
{
    [Activity(
        Name = "com.xamarin.essentials.devicetests.MainActivity",
        Label = "@string/app_name",
        Theme = "@style/MainTheme",
        MainLauncher = true,
        ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
    public class MainActivity : RunnerActivity
    {
        protected override void OnCreate(Bundle bundle)
        {
            Xamarin.Essentials.Platform.Init(this, bundle);

            // tests can be inside the main assembly
            AddTestAssembly(Assembly.GetExecutingAssembly());
            AddTestAssembly(typeof(Battery_Tests).Assembly);
            AddExecutionAssembly(typeof(Battery_Tests).Assembly);

            base.OnCreate(bundle);
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}
