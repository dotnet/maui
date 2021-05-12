using Android.App;
using Android.OS;
using Xunit.Runners.UI;
using System.Reflection;
using Android.Content.PM;

namespace Microsoft.Maui.DeviceTests
{
	[Activity(
		Name = "com.microsoft.maui.compatibility.devicetests.MainActivity",
		Label = "Compat Tests",
		Theme = "@style/Maui.MainTheme",
		MainLauncher = true,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : RunnerActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			Microsoft.Maui.Essentials.Platform.Init(this, bundle);
			Platform.Init(this);

			// Tests can be inside the main assembly
			AddTestAssembly(typeof(MainActivity).Assembly);
			AddTestAssembly(typeof(CompatTests).Assembly);
			AddTestAssembly(typeof(CoreTests).Assembly);

			base.OnCreate(bundle);
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			Microsoft.Maui.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}
	}
}