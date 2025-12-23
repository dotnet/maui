using Microsoft.Maui.IntegrationTests.Apple;

namespace Microsoft.Maui.IntegrationTests
{
	[Trait("Category", Categories.RunOniOS)]
	public class AppleTemplateTests : BaseBuildTest, IDisposable
	{
		Simulator TestSimulator = new Simulator();

		public AppleTemplateTests(BuildTestFixture fixture, ITestOutputHelper output) : base(fixture, output)
		{
			if (!TestEnvironment.IsMacOS)
				return; // Skip on non-macOS

			TestSimulator.Shutdown();
			if (!TestSimulator.Launch())
				throw new Exception($"Failed to boot simulator with UDID '{TestSimulator.GetUDID()}'.");
			TestSimulator.ShowWindow();
		}

		public new void Dispose()
		{
			TestSimulator.Shutdown();
			base.Dispose();
		}

		[Theory]
		// [InlineData("maui", "Debug", DotNetPrevious, RuntimeVariant.Mono, null)]
		// [InlineData("maui", "Release", DotNetPrevious, RuntimeVariant.Mono, null)]
		[InlineData("maui", "Debug", DotNetCurrent, RuntimeVariant.Mono, null)]
		[InlineData("maui", "Release", DotNetCurrent, RuntimeVariant.Mono, null)]
		[InlineData("maui", "Release", DotNetCurrent, RuntimeVariant.Mono, "full")]
		// [InlineData("maui-blazor", "Debug", DotNetPrevious, RuntimeVariant.Mono, null)]
		// [InlineData("maui-blazor", "Release", DotNetPrevious, RuntimeVariant.Mono, null)]
		[InlineData("maui-blazor", "Debug", DotNetCurrent, RuntimeVariant.Mono, null)]
		[InlineData("maui-blazor", "Release", DotNetCurrent, RuntimeVariant.Mono, null)]
		[InlineData("maui-blazor", "Release", DotNetCurrent, RuntimeVariant.Mono, "full")]
		[InlineData("maui", "Release", DotNetCurrent, RuntimeVariant.NativeAOT, null)]
		public void RunOniOS(string id, string config, string framework, RuntimeVariant runtimeVariant, string? trimMode)
		{
			if (!TestEnvironment.IsMacOS)
				return; // Skip: Running Apple templates is only supported on macOS.

			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.True(DotnetInternal.New(id, projectDir, framework),
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

			Assert.True(DotnetInternal.Build(projectFile, config, framework: $"{framework}-ios", properties: buildProps),
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
			Assert.True(XHarness.RunAppleForTimeout(appFile, xhResultsDir, TestSimulator.XHarnessID, TestSimulator.GetUDID()),
				$"Project {Path.GetFileName(projectFile)} failed to run. Check test output/attachments for errors.");
		}
	}
}
