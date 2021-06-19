using System;
using Microsoft.CodeAnalysis;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.SourceGen
{
	class VisualRunnerGenerator : RunnerGenerator
	{
		public VisualRunnerGenerator(GeneratorExecutionContext context, string targetFramework)
			: base(context, targetFramework)
		{
		}

		public override void Generate()
		{
			if (TargetFramework.IndexOf("-android", StringComparison.OrdinalIgnoreCase) != -1)
			{
				var code = GenerateAndroidSource();
				var name = "VisualRunner.Android.sg.cs";

				AddSource(name, code);
			}
			else if (TargetFramework.IndexOf("-ios", StringComparison.OrdinalIgnoreCase) != -1)
			{
				var code = GenerateIosSource();
				var name = "VisualRunner.iOS.sg.cs";

				AddSource(name, code);
			}
			else if (TargetFramework.IndexOf("-maccatalyst", StringComparison.OrdinalIgnoreCase) != -1)
			{
				var code = GenerateIosSource();
				var name = "VisualRunner.MacCatalyst.sg.cs";

				AddSource(name, code);
			}
		}

		string GenerateAndroidSource()
		{
			var ns = RootNamespace;
			var startupName = "Startup";
			var appName = "MainApplication";
			var activityName = "MainActivity";
			var splash = ContainsSplashScreen ? @"Theme = ""@style/Maui.SplashTheme""," : "";

			return @"
#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_RUNNER_APPLICATION_GENERATION
namespace " + ns + @"
{
	[global::Android.App.Application]
	partial class " + appName + @" : global::Microsoft.Maui.MauiApplication<global::" + ns + @"." + startupName + @">
	{
		public " + appName + @"(global::System.IntPtr handle, global::Android.Runtime.JniHandleOwnership ownership)
			: base(handle, ownership)
		{
		}
	}
}
#endif

#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_RUNNER_ACTIVITY_GENERATION
namespace " + ns + @"
{
	[global::Android.App.Activity(
		" + splash + @"
		MainLauncher = true,
		ConfigurationChanges =
			global::Android.Content.PM.ConfigChanges.ScreenSize |
			global::Android.Content.PM.ConfigChanges.Orientation |
			global::Android.Content.PM.ConfigChanges.UiMode |
			global::Android.Content.PM.ConfigChanges.ScreenLayout |
			global::Android.Content.PM.ConfigChanges.SmallestScreenSize)]
	partial class " + activityName + @" : global::Microsoft.Maui.MauiAppCompatActivity
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
