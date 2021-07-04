using System.Linq;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Xunit.Runners.UI;

namespace Microsoft.Maui.DeviceTests
{
	[Activity(
		Name = "com.microsoft.maui.devicetests.MainActivity",
		Label = "@string/app_name",
		Theme = "@style/MainTheme",
		MainLauncher = true,
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class MainActivity : RunnerActivity
	{
		protected override void OnCreate(Bundle bundle)
		{
			Microsoft.Maui.Essentials.Platform.Init(this, bundle);
			Platform.Init(this);

			foreach (var assembly in TestInstrumentation.TestAssemblies.Distinct())
				AddTestAssembly(assembly);

			base.OnCreate(bundle);
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, Permission[] grantResults)
		{
			Microsoft.Maui.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}
	}
}