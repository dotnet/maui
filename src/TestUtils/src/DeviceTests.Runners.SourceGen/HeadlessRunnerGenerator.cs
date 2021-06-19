using System;
using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.SourceGen
{
	class HeadlessRunnerGenerator : RunnerGenerator
	{
		public HeadlessRunnerGenerator(GeneratorExecutionContext context, string targetFramework)
			: base(context, targetFramework)
		{
		}

		public override void Generate()
		{
			if (TargetFramework.IndexOf("-android", StringComparison.OrdinalIgnoreCase) != -1)
			{
				var code = GenerateAndroidSource();
				var name = "HeadlessRunner.Android.sg.cs";

				AddSource(name, code);
			}
			//else if (TargetFramework.IndexOf("-ios", StringComparison.OrdinalIgnoreCase) != -1)
			//{
			//	var code = GenerateIosSource();
			//	var name = "HeadlessRunner.iOS.sg.cs";

			//	AddSource(name, code);
			//}
			//else if (TargetFramework.IndexOf("-maccatalyst", StringComparison.OrdinalIgnoreCase) != -1)
			//{
			//	var code = GenerateIosSource();
			//	var name = "HeadlessRunner.MacCatalyst.sg.cs";

			//	AddSource(name, code);
			//}
		}

		string GenerateAndroidSource()
		{
			var startupName = "Startup";
			var instrumentationName = "MainInstrumentation";
			var activityName = "TestActivity";
			var splash = ContainsSplashScreen ? @"Theme = ""@style/Maui.SplashTheme""," : "";

			return @"
#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_HEADLESS_RUNNER_ENTRYPOINT_GENERATION && !SKIP_HEADLESS_RUNNER_INSTRUMENTATION_GENERATION
namespace " + RootNamespace + @"
{
	[global::Android.App.Instrumentation(Name = " + ApplicationId + @")]
	public partial class " + instrumentationName + @" : global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.MauiTestInstrumentation<global::" + RootNamespace + @"." + startupName + @", global::" + RootNamespace + @"." + activityName + @">
	{
		protected " + instrumentationName + @"(global::System.IntPtr handle, global::Android.Runtime.JniHandleOwnership transfer)
			: base(handle, transfer)
		{
		}
	}
}
#endif

#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_HEADLESS_RUNNER_ENTRYPOINT_GENERATION && !SKIP_HEADLESS_RUNNER_ACTIVITY_GENERATION
namespace " + RootNamespace + @"
{
	[global::Android.App.Activity(
		" + splash + @"
		ConfigurationChanges =
			global::Android.Content.PM.ConfigChanges.ScreenSize |
			global::Android.Content.PM.ConfigChanges.Orientation |
			global::Android.Content.PM.ConfigChanges.UiMode |
			global::Android.Content.PM.ConfigChanges.ScreenLayout |
			global::Android.Content.PM.ConfigChanges.SmallestScreenSize)]
	public partial class " + activityName + @" : global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.MauiTestAppCompatActivity
	{
	}
}
#endif
";
		}

		string GenerateIosSource()
		{
			var ns = RootNamespace;
			var startupName = "Startup";
			var delegateName = "AppDelegate";

			return @"
#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_RUNNER_PROGRAM_GENERATION
namespace " + ns + @"
{
	partial class Program
	{
		static void Main(string[] args)
		{
			global::UIKit.UIApplication.Main(args, null, nameof(global::" + ns + @"." + delegateName + @"));
		}
	}
}
#endif

#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_RUNNER_APP_DELEGATE_GENERATION
namespace " + ns + @"
{
	[global::Foundation.Register(nameof(" + delegateName + @"))]
	partial class " + delegateName + @" : global::Microsoft.Maui.MauiUIApplicationDelegate<global::" + ns + @"." + startupName + @">
	{
	}
}
#endif
";
		}
	}
}
