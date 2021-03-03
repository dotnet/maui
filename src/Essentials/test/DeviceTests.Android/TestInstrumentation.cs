using System;
using System.Collections.Generic;
using System.Reflection;
using Android.App;
using Android.Runtime;
using Microsoft.DotNet.XHarness.TestRunners.Common;
using Microsoft.Maui.TestUtils;

namespace Microsoft.Maui.Essentials.DeviceTests.Droid
{
	[Instrumentation(Name = "com.microsoft.maui.essentials.devicetests.TestInstrumentation")]
	public class TestInstrumentation : BaseTestInstrumentation
	{
		public TestInstrumentation(IntPtr handle, JniHandleOwnership transfer)
			: base(handle, transfer)
		{
		}

		public override IEnumerable<TestAssemblyInfo> GetTestAssemblies()
		{
			yield return new TestAssemblyInfo(Assembly.GetExecutingAssembly(), Assembly.GetExecutingAssembly().Location);
			yield return new TestAssemblyInfo(typeof(Battery_Tests).Assembly, typeof(Battery_Tests).Assembly.Location);
		}

		public override TestRunner GetTestRunner(TestRunner testRunner, LogWriter logWriter)
		{
			var additional = new List<string>
			{
				$"{Traits.FileProvider}={Traits.FeatureSupport.ToExclude(Platform.HasApiLevel(24))}",
			};
			testRunner.SkipCategories(Traits.GetSkipTraits(additional));

			return testRunner;
		}
	}
}