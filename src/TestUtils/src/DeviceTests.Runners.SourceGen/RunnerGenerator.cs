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
			ApplicationId = applicationId ?? throw new InvalidOperationException("ApplicationId needs to be set.");
			context.AnalyzerConfigOptions.GlobalOptions.TryGetValue("build_property.RootNamespace", out var rootNamespace);
			RootNamespace = rootNamespace ?? "TestRunnerNamespace";
		}

		public GeneratorExecutionContext Context { get; }
		public string TargetFramework { get; }
		public string RootNamespace { get; }
		public string ApplicationId { get; }

		public void Generate()
		{
			if (TargetFramework.IndexOf("-android", StringComparison.OrdinalIgnoreCase) >= 0)
				AddSource("TestRunner.Android.sg.cs", GenerateAndroidSource());
			else if (TargetFramework.IndexOf("-ios", StringComparison.OrdinalIgnoreCase) >= 0 ||
				TargetFramework.IndexOf("-maccatalyst", StringComparison.OrdinalIgnoreCase) >= 0)
				AddSource("TestRunner.Apple.sg.cs", GenerateAppleSource());
		}

		void AddSource(string filename, string source) =>
			Context.AddSource(filename, SourceText.From(source, Encoding.UTF8));

		string GenerateAndroidSource() => $$"""
			#if !SKIP_RUNNER_ENTRYPOINT_GENERATION
			namespace {{RootNamespace}}
			{
				[global::Android.App.Application]
				partial class MainApplication : global::Microsoft.Maui.MauiApplication
				{
					public MainApplication(global::System.IntPtr handle, global::Android.Runtime.JniHandleOwnership ownership)
						: base(handle, ownership) { }

					protected override global::Microsoft.Maui.Hosting.MauiApp CreateMauiApp() =>
						global::{{RootNamespace}}.MauiProgram.CreateMauiApp();
				}

				[global::Android.App.Instrumentation(Name = "{{ApplicationId}}.TestInstrumentation")]
				public partial class TestInstrumentation :
					global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.MauiTestInstrumentation
				{
					protected TestInstrumentation(global::System.IntPtr handle, global::Android.Runtime.JniHandleOwnership ownership)
						: base(handle, ownership) { }
				}

				[global::Android.App.Activity(
					Name = "{{ApplicationId}}.TestActivity",
					Theme = "@style/Maui.MainTheme.NoActionBar",
					ConfigurationChanges =
						global::Android.Content.PM.ConfigChanges.ScreenSize |
						global::Android.Content.PM.ConfigChanges.Orientation |
						global::Android.Content.PM.ConfigChanges.UiMode |
						global::Android.Content.PM.ConfigChanges.ScreenLayout |
						global::Android.Content.PM.ConfigChanges.SmallestScreenSize)]
				public partial class TestActivity :
					global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.MauiTestActivity
				{
				}
			}
			#endif
			""";

		string GenerateAppleSource() => $$"""
			#if !SKIP_RUNNER_ENTRYPOINT_GENERATION
			namespace {{RootNamespace}}
			{
				#if !SKIP_RUNNER_PROGRAM_GENERATION
				partial class Program
				{
					static void Main(global::System.String[] args) =>
						global::UIKit.UIApplication.Main(args, null, typeof(HeadlessRunnerAppDelegate));
				}
				#endif

				[global::Foundation.Register("HeadlessRunnerAppDelegate")]
				partial class HeadlessRunnerAppDelegate :
					global::Microsoft.Maui.TestUtils.DeviceTests.Runners.HeadlessRunner.MauiTestApplicationDelegate
				{
					protected override global::Microsoft.Maui.Hosting.MauiApp CreateMauiApp() =>
						global::{{RootNamespace}}.MauiProgram.CreateMauiApp();
				}
			}
			#endif
			""";
	}
}
