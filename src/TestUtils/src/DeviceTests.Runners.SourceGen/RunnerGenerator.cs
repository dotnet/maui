using System;
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
			var mauiProgramName = "MauiProgram";
			var mauiProgramFullName = @"global::" + RootNamespace + "." + mauiProgramName;
			var splash = ContainsSplashScreen ? @"Theme = ""@style/Maui.SplashTheme""," : "";

			var appName = "MainApplication";
			var visualActivityName = "MainActivity";

			var instrumentationName = "TestInstrumentation";
			var headlessActivityName = "TestActivity";

			return @"
#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_VISUAL_RUNNER_ENTRYPOINT_GENERATION && !SKIP_VISUAL_RUNNER_APPLICATION_GENERATION
namespace " + RootNamespace + @"
{
	[global::Android.App.Application]
	partial class " + appName + @" : global::Microsoft.Maui.MauiApplication
	{
		public " + appName + @"(global::System.IntPtr handle, global::Android.Runtime.JniHandleOwnership ownership)
			: base(handle, ownership)
		{
		}

		protected override global::Microsoft.Maui.Hosting.MauiApp CreateMauiApp() => " + mauiProgramFullName + @".CreateMauiApp();
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
	[global::Android.App.Instrumentation(Name = """ + ApplicationId + "." + instrumentationName + @""")]
	public partial class " + instrumentationName + @" : global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.MauiTestInstrumentation
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
		Name = """ + ApplicationId + "." + headlessActivityName + @""",
		Theme = ""@style/Maui.MainTheme.NoActionBar"",
		ConfigurationChanges =
			global::Android.Content.PM.ConfigChanges.ScreenSize |
			global::Android.Content.PM.ConfigChanges.Orientation |
			global::Android.Content.PM.ConfigChanges.UiMode |
			global::Android.Content.PM.ConfigChanges.ScreenLayout |
			global::Android.Content.PM.ConfigChanges.SmallestScreenSize)]
	public partial class " + headlessActivityName + @" : global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.MauiTestActivity
	{
	}
}
#endif
";
		}

		string GenerateIosSource()
		{
			var mauiProgramName = "MauiProgram";
			var mauiProgramFullName = @"global::" + RootNamespace + "." + mauiProgramName;
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
#if !SKIP_HEADLESS_RUNNER_APP_DELEGATE_GENERATION
			if (global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.MauiTestApplicationDelegate.IsHeadlessRunner(args))
			{
				global::UIKit.UIApplication.Main(args, null, typeof(global::" + RootNamespace + @"." + headlessDelegateName + @"));
			}
			else
#endif
			{
#if !SKIP_VISUAL_RUNNER_APP_DELEGATE_GENERATION
				global::UIKit.UIApplication.Main(args, null, typeof(global::" + RootNamespace + @"." + visualDelegateName + @"));
#endif
			}
		}
	}
}
#endif

#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_VISUAL_RUNNER_ENTRYPOINT_GENERATION && !SKIP_VISUAL_RUNNER_APP_DELEGATE_GENERATION
namespace " + RootNamespace + @"
{
	[global::Foundation.Register(""" + visualDelegateName + @""")]
	partial class " + visualDelegateName + @" : global::Microsoft.Maui.MauiUIApplicationDelegate
	{
		protected override global::Microsoft.Maui.Hosting.MauiApp CreateMauiApp() => " + mauiProgramFullName + @".CreateMauiApp();
	}
}
#endif

#if !SKIP_RUNNER_ENTRYPOINT_GENERATION && !SKIP_HEADLESS_RUNNER_ENTRYPOINT_GENERATION && !SKIP_HEADLESS_RUNNER_APP_DELEGATE_GENERATION
namespace " + RootNamespace + @"
{
	[global::Foundation.Register(""" + headlessDelegateName + @""")]
	partial class " + headlessDelegateName + @" : global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.MauiTestApplicationDelegate
	{

		protected override global::Microsoft.Maui.Hosting.MauiApp CreateMauiApp() => " + mauiProgramFullName + @".CreateMauiApp();
	}
}
#endif
";
		}
	}
}