using System;
using System.Collections.Generic;
using System.Reflection;
using Android.App;
using Android.Content.PM;
using Android.OS;
using Android.Runtime;
using Microsoft.DotNet.XHarness.TestRunners.Common;
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
			yield return new TestAssemblyInfo(Assembly.GetExecutingAssembly(), Assembly.GetExecutingAssembly().Location);
			yield return new TestAssemblyInfo(typeof(SliderHandlerTests).Assembly, typeof(SliderHandlerTests).Assembly.Location);
		}
	}

	[Instrumentation(Name = "com.microsoft.maui.devicetests.TestInstrumentation")]
	public class TestInstrumentation : BaseTestInstrumentation<TestActivity>
	{
		protected TestInstrumentation(IntPtr handle, JniHandleOwnership transfer)
			: base(handle, transfer)
		{
		}
	}
}