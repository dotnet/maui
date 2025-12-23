
using Microsoft.Maui.IntegrationTests.Apple;

namespace Microsoft.Maui.IntegrationTests
{
	public class AppleTemplateTests : BaseBuildTest
	{
		Simulator TestSimulator = new Simulator();

		[SetUp]
		public void AppleTemplateSetup()
		{
			if (!TestEnvironment.IsMacOS)
				Assert.Ignore("Running Apple templates is only supported on macOS.");

			TestSimulator.Shutdown();
			Assert.IsTrue(TestSimulator.Launch(), $"Failed to boot simulator with UDID '{TestSimulator.GetUDID()}'.");
			TestSimulator.ShowWindow();
		}

		[OneTimeTearDown]
		public void AppleTemplateFxtTearDown()
		{
			TestSimulator.Shutdown();
		}

		// Individual test methods for each configuration to enable parallel CI runs
		[Test, Category(Categories.RunOniOS), Category(Categories.RunOniOS_MauiDebug)]
		public void RunOniOS_MauiDebug() => RunOniOS("maui", "Debug", DotNetCurrent, RuntimeVariant.Mono, null);

		[Test, Category(Categories.RunOniOS), Category(Categories.RunOniOS_MauiRelease)]
		public void RunOniOS_MauiRelease() => RunOniOS("maui", "Release", DotNetCurrent, RuntimeVariant.Mono, null);

		[Test, Category(Categories.RunOniOS), Category(Categories.RunOniOS_MauiReleaseTrimFull)]
		public void RunOniOS_MauiReleaseTrimFull() => RunOniOS("maui", "Release", DotNetCurrent, RuntimeVariant.Mono, "full");

		[Test, Category(Categories.RunOniOS), Category(Categories.RunOniOS_BlazorDebug)]
		public void RunOniOS_BlazorDebug() => RunOniOS("maui-blazor", "Debug", DotNetCurrent, RuntimeVariant.Mono, null);

		[Test, Category(Categories.RunOniOS), Category(Categories.RunOniOS_BlazorRelease)]
		public void RunOniOS_BlazorRelease() => RunOniOS("maui-blazor", "Release", DotNetCurrent, RuntimeVariant.Mono, null);

		[Test, Category(Categories.RunOniOS), Category(Categories.RunOniOS_BlazorReleaseTrimFull)]
		public void RunOniOS_BlazorReleaseTrimFull() => RunOniOS("maui-blazor", "Release", DotNetCurrent, RuntimeVariant.Mono, "full");

		[Test, Category(Categories.RunOniOS), Category(Categories.RunOniOS_MauiNativeAOT)]
		public void RunOniOS_MauiNativeAOT() => RunOniOS("maui", "Release", DotNetCurrent, RuntimeVariant.NativeAOT, null);

		void RunOniOS(string id, string config, string framework, RuntimeVariant runtimeVariant, string? trimMode)
		{
			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
				$"Unable to create template {id}. Check test output for errors.");

			var buildProps = BuildProps;

			if (runtimeVariant == RuntimeVariant.NativeAOT)
			{
				buildProps.Add("PublishAot=true");
				buildProps.Add("PublishAotUsingRuntimePack=true"); // TODO: This parameter will become obsolete https://github.com/dotnet/runtime/issues/87060
				buildProps.Add("_IsPublishing=true"); // using dotnet build with -p:_IsPublishing=true enables targeting simulators
				buildProps.Add("IlcTreatWarningsAsErrors=false"); // TODO: Remove this once all warnings are fixed https://github.com/dotnet/maui/issues/19397
			}

			if (!string.IsNullOrEmpty(trimMode))
			{
				buildProps.Add($"TrimMode={trimMode}");
				buildProps.Add("TrimmerSingleWarn=false"); // Disable trimmer warnings for iOS full trimming builds due to ObjCRuntime issues
			}

			Assert.IsTrue(DotnetInternal.Build(projectFile, config, framework: $"{framework}-ios", properties: buildProps),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

			// Find the .app bundle - it may be in the bin folder with or without a RID subfolder depending on build settings
			var appName = $"{Path.GetFileName(projectDir)}.app";
			var binDir = Path.Combine(projectDir, "bin", config, $"{framework}-ios");
			var appFile = Directory.GetDirectories(binDir, appName, SearchOption.AllDirectories).FirstOrDefault()
				?? Path.Combine(binDir, appName);

			// Write xh-results to the log directory for artifact collection
			var xhResultsDir = Path.Combine(TestEnvironment.GetLogDirectory(), "xh-results", Path.GetFileName(projectDir));
			Directory.CreateDirectory(xhResultsDir);

			// Pass the device UDID to use the already-booted simulator directly.
			// This prevents XHarness from managing the simulator lifecycle and avoids race conditions
			// where XHarness might shut down the simulator before launching the app.
			Assert.IsTrue(XHarness.RunAppleForTimeout(appFile, xhResultsDir, TestSimulator.XHarnessID, TestSimulator.GetUDID()),
				$"Project {Path.GetFileName(projectFile)} failed to run. Check test output/attachments for errors.");
		}
	}
}
