namespace NewNamespace
{
	[global::Android.App.Activity(
		Theme = "@style/MainTheme",
		ConfigurationChanges =
			global::Android.Content.PM.ConfigChanges.ScreenSize |
			global::Android.Content.PM.ConfigChanges.Orientation)]
	public class TestActivity : global::Microsoft.Maui.TestUtils.MauiTestAppCompatActivity
	{
		public override global::System.Collections.Generic.IEnumerable<global::Microsoft.DotNet.XHarness.TestRunners.Common.TestAssemblyInfo> GetTestAssemblies()
		{
			foreach (var assembly in TestInstrumentation.TestAssemblies)
			{
				var path = global::System.IO.Path.Combine(CacheDir.AbsolutePath, assembly.GetName().Name);
				global::System.IO.File.Create(path).Close();

				yield return new global::Microsoft.DotNet.XHarness.TestRunners.Common.TestAssemblyInfo(assembly, path);
			}
		}
	}

	[global::Android.App.Instrumentation(Name = "com.microsoft.maui.core.devicetests.TestInstrumentation")]
	public class TestInstrumentation : global::Microsoft.Maui.TestUtils.MauiTestInstrumentation<global::Microsoft.Maui.Core.DeviceTests.Startup>
	{
		protected TestInstrumentation(global::System.IntPtr handle, global::Android.Runtime.JniHandleOwnership transfer)
			: base(handle, transfer)
		{
		}
	}
}