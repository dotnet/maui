using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.TestUtils;

namespace Microsoft.Maui.DeviceTests
{
	[Activity(
		Name = "com.microsoft.maui.devicetests.TestActivity",
		Label = "@string/app_name",
		Theme = "@style/MainTheme",
		ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation)]
	public class TestActivity : BaseTestActivity
	{
		protected override void OnCreate(Bundle savedInstanceState)
		{
			Platform.Init(this);

			base.OnCreate(savedInstanceState);
		}

		public override IEnumerable<TestAssemblyInfo> GetTestAssemblies()
		{
			foreach (var assembly in TestInstrumentation.TestAssemblies.Distinct())
			{
				// this is required to exist, but is not used
				var path = Path.Combine(FileSystem.CacheDirectory, assembly.GetName().Name);
				File.Create(path).Close();

				yield return new TestAssemblyInfo(assembly, path);
			}
		}
	}

	[Instrumentation(Name = "com.microsoft.maui.devicetests.TestInstrumentation")]
	public class TestInstrumentation : BaseTestInstrumentation<TestActivity>
	{
		protected TestInstrumentation(IntPtr handle, JniHandleOwnership transfer)
			: base(handle, transfer)
		{
		}

		public static IEnumerable<Assembly> TestAssemblies
		{
			get
			{
				yield return Assembly.GetExecutingAssembly();
				yield return typeof(SliderHandlerTests).Assembly;
			}
		}
	}
}