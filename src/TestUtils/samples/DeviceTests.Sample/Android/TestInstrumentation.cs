namespace NewNamespace
{
	[global::Android.App.Activity(
		Theme = "@style/Maui.SplashTheme",
		MainLauncher = true,
		ConfigurationChanges =
			global::Android.Content.PM.ConfigChanges.ScreenSize |
			global::Android.Content.PM.ConfigChanges.Orientation |
			global::Android.Content.PM.ConfigChanges.UiMode |
			global::Android.Content.PM.ConfigChanges.ScreenLayout |
			global::Android.Content.PM.ConfigChanges.SmallestScreenSize)]
	public class TestActivity : global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.MauiTestAppCompatActivity
	{
	}

	[global::Android.App.Instrumentation(Name = "com.microsoft.maui.testutils.devicetests.TestInstrumentation")]
	public class TestInstrumentation : global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.MauiTestInstrumentation<global::Microsoft.Maui.TestUtils.DeviceTests.Sample.Startup, global::NewNamespace.TestActivity>
	{
		protected TestInstrumentation(global::System.IntPtr handle, global::Android.Runtime.JniHandleOwnership transfer)
			: base(handle, transfer)
		{
		}
	}
}