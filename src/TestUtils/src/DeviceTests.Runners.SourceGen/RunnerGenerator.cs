using System;
using System.IO;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Microsoft.Maui.TestUtils.DeviceTests.Runners.SourceGen
{
	public class RunnerGenerator
	{
		public RunnerGenerator(GeneratorExecutionContext context, string targetFramework)
		{
			Context = context;

			TargetFramework = targetFramework;

			context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.ApplicationId", out var applicationId);
			context.Log($"ApplicationId: {applicationId}");
			ApplicationId = applicationId ?? throw new Exception("ApplicationId needs to be set.");

			context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.ApplicationTitle", out var applicationTitle);
			context.Log($"ApplicationTitle: {applicationTitle}");
			ApplicationTitle = applicationTitle ?? "Tests";

			context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);
			context.Log($"RootNamespace: {rootNamespace}");
			RootNamespace = rootNamespace ?? "TestRunnerNamespace";

			ContainsSplashScreen = false;
			foreach (var file in context.AdditionalFiles)
			{
				var options = context.AnalyzerConfigOptions.GetOptions(file);
				if (options.TryGetValue("build_metadata.AdditionalFiles.IsMauiSplashScreen", out var isMauiSplashScreen) && bool.TryParse(isMauiSplashScreen, out var isSplash) && isSplash)
				{
					ContainsSplashScreen = true;
					break;
				}
			}
			context.Log($"ContainsSplashScreen: {ContainsSplashScreen}");
		}

		public GeneratorExecutionContext Context { get; }

		public string TargetFramework { get; }

		public string RootNamespace { get; }

		public string ApplicationId { get; }

		public string ApplicationTitle { get; }

		public bool ContainsSplashScreen { get; }

		public void Generate()
		{
			Context.Log($"Generating runners...");

			if (TargetFramework.IndexOf("-android", StringComparison.OrdinalIgnoreCase) != -1)
			{
				var code = GenerateAndroidSource();
				var name = "TestRunner.Android.sg.cs";

				AddSource(name, code);
			}
			else if (TargetFramework.IndexOf("-ios", StringComparison.OrdinalIgnoreCase) != -1)
			{
				var code = GenerateIosSource();
				var name = "TestRunner.iOS.sg.cs";

				AddSource(name, code);
			}
			else if (TargetFramework.IndexOf("-maccatalyst", StringComparison.OrdinalIgnoreCase) != -1)
			{
				var code = GenerateIosSource();
				var name = "TestRunner.MacCatalyst.sg.cs";

				AddSource(name, code);
			}
		}

		protected void AddSource(string filename, string contents)
		{
			Context.Log($"AddSource: {filename}");
			Context.AddSource(filename, SourceText.From(contents, Encoding.UTF8));
		}

		string GenerateAndroidSource()
		{
			var startupName = "Startup";
			var splash = ContainsSplashScreen ? @"Theme = ""@style/Maui.SplashTheme""," : "";

			var appName = "MainApplication";
			var visualActivityName = "MainActivity";

			var instrumentationName = "MainInstrumentation";
			var headlessActivityName = "TestActivity";

			return @"
#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_VISUAL_RUNNER_ENTRYPOINT_GENERATION && !SKIP_VISUAL_RUNNER_APPLICATION_GENERATION
namespace " + RootNamespace + @"
{
	[global::Android.App.Application]
	partial class " + appName + @" : global::Microsoft.Maui.MauiApplication<global::" + RootNamespace + @"." + startupName + @">
	{
		public " + appName + @"(global::System.IntPtr handle, global::Android.Runtime.JniHandleOwnership ownership)
			: base(handle, ownership)
		{
		}
	}
}
#endif

#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_VISUAL_RUNNER_ENTRYPOINT_GENERATION && !SKIP_VISUAL_RUNNER_ACTIVITY_GENERATION
namespace " + RootNamespace + @"
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
	partial class " + visualActivityName + @" : global::Microsoft.Maui.MauiAppCompatActivity
	{
	}
}
#endif

#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_HEADLESS_RUNNER_ENTRYPOINT_GENERATION && !SKIP_HEADLESS_RUNNER_INSTRUMENTATION_GENERATION
namespace " + RootNamespace + @"
{
	[global::Android.App.Instrumentation(Name = " + ApplicationId + @")]
	public partial class " + instrumentationName + @" : global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.MauiTestInstrumentation<global::" + RootNamespace + @"." + startupName + @", global::" + RootNamespace + @"." + headlessActivityName + @">
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
	public partial class " + headlessActivityName + @" : global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.MauiTestAppCompatActivity
	{
	}
}
#endif
";
		}

		string GenerateIosSource()
		{
			var startupName = "Startup";
			var visualDelegateName = "VisualRunnerAppDelegate";
			var headlessDelegateName = "HeadlessRunnerAppDelegate";

			return @"
#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_VISUAL_RUNNER_ENTRYPOINT_GENERATION && !SKIP_RUNNER_PROGRAM_GENERATION
namespace " + RootNamespace + @"
{
	partial class Program
	{
		static void Main(global::System.String[] args)
		{
#if !SKIP_HEADLESS_RUNNER_ENTRYPOINT_GENERATION
			if (global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.MauiTestApplicationDelegate.IsHeadlessRunner(args))
			{
				global::UIKit.UIApplication.Main(args, null, nameof(global::" + RootNamespace + @"." + headlessDelegateName + @"));
			}
			else
#endif
			{
#if !SKIP_VISUAL_RUNNER_APP_DELEGATE_GENERATION
				global::UIKit.UIApplication.Main(args, null, nameof(global::" + RootNamespace + @"." + visualDelegateName + @"));
#endif
			}
		}
	}
}
#endif

#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_VISUAL_RUNNER_ENTRYPOINT_GENERATION && !SKIP_VISUAL_RUNNER_APP_DELEGATE_GENERATION
namespace " + RootNamespace + @"
{
	[global::Foundation.Register(nameof(" + visualDelegateName + @"))]
	partial class " + visualDelegateName + @" : global::Microsoft.Maui.MauiUIApplicationDelegate<global::" + RootNamespace + @"." + startupName + @">
	{
	}
}
#endif

#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_HEADLESS_RUNNER_ENTRYPOINT_GENERATION && !SKIP_HEADLESS_RUNNER_APP_DELEGATE_GENERATION
namespace " + RootNamespace + @"
{
	[global::Foundation.Register(nameof(" + headlessDelegateName + @"))]
	partial class " + headlessDelegateName + @" : global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.MauiTestApplicationDelegate<global::" + RootNamespace + @"." + startupName + @">
	{
	}
}
#endif
";
		}
	}
}