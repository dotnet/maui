
using Microsoft.Maui.IntegrationTests.Apple;

namespace Microsoft.Maui.IntegrationTests
{
	[Category(Categories.RunOniOS)]
	public class AppleTemplateTests : BaseBuildTest
	{
		Simulator TestSimulator = new Simulator();

		[SetUp]
		public void AppleTemplateSetup()
		{
			if (!TestEnvironment.IsMacOS)
				Assert.Ignore("Running Apple templates is only supported on macOS.");

			// Pre-boot the simulator before XHarness runs.
			// This ensures the full timeout is available for install + run, not consumed by boot time.
			// Without this, booting a shutdown simulator (~30-35s on CI) can exhaust the timeout
			// before the app even gets installed.
			TestSimulator.Shutdown();
			Assert.IsTrue(TestSimulator.Launch(), 
				$"Failed to boot simulator. Target: {TestSimulator.XHarnessID}, UDID: {TestSimulator.GetUDID()}");
		}

		[OneTimeTearDown]
		public void AppleTemplateFxtTearDown()
		{
			// Shutdown simulator after all tests complete to clean up resources.
			TestSimulator.Shutdown();
		}

		// [TestCase("maui", "Debug", DotNetPrevious, "iossimulator-x64", RuntimeVariant.Mono, null)]
		// [TestCase("maui", "Release", DotNetPrevious, "iossimulator-x64", RuntimeVariant.Mono, null)]
		// [TestCase("maui-blazor", "Debug", DotNetPrevious, "iossimulator-x64", RuntimeVariant.Mono, null)]
		// [TestCase("maui-blazor", "Release", DotNetPrevious, "iossimulator-x64", RuntimeVariant.Mono, null)]
		

		// Individual test methods for each configuration to enable parallel CI runs
		// CI uses --filter "Name=TestMethodName" to run each test in a separate job
		[Test]
		public void RunOniOS_MauiDebug() => RunOniOS("maui", "Debug", DotNetCurrent, RuntimeVariant.Mono, null);

		[Test]
		public void RunOniOS_MauiRelease() => RunOniOS("maui", "Release", DotNetCurrent, RuntimeVariant.Mono, null);

		[Test]
		public void RunOniOS_MauiReleaseTrimFull() => RunOniOS("maui", "Release", DotNetCurrent, RuntimeVariant.Mono, "full");

		[Test]
		public void RunOniOS_BlazorDebug() => RunOniOS("maui-blazor", "Debug", DotNetCurrent, RuntimeVariant.Mono, null);

		[Test]
		public void RunOniOS_BlazorRelease() => RunOniOS("maui-blazor", "Release", DotNetCurrent, RuntimeVariant.Mono, null);

		// TODO: Re-enable once ASP.NET Core fixes trimmer warning IL2111 with Blazor Router.NotFoundPage
		// Issue: https://github.com/dotnet/aspnetcore/issues/63951
		// 
		// When building maui-blazor template with TrimMode=full, the Razor source generator produces code
		// that accesses Router.NotFoundPage via reflection, triggering IL2111:
		//
		//   error IL2111: Method 'Microsoft.AspNetCore.Components.Routing.Router.NotFoundPage.set' with
		//   parameters or return value with `DynamicallyAccessedMembersAttribute` is accessed via reflection.
		//   Trimmer can't guarantee availability of the requirements of the method.
		//
		// This is a known limitation - Blazor doesn't fully support TrimMode=full for application assemblies.
		// See: https://learn.microsoft.com/en-us/aspnet/core/blazor/host-and-deploy/configure-trimmer
		//
		// [Test, Category(Categories.RunOniOS)]
		// public void RunOniOS_BlazorReleaseTrimFull() => RunOniOS("maui-blazor", "Release", DotNetCurrent, RuntimeVariant.Mono, "full");

		[Test]
		public void RunOniOS_MauiNativeAOT() => RunOniOS("maui", "Release", DotNetCurrent, RuntimeVariant.NativeAOT, null);

		void RunOniOS(string id, string config, string framework, RuntimeVariant runtimeVariant, string? trimMode)
		{
			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
				$"Unable to create template {id}. Check test output for errors.");

			var buildProps = BuildProps;
			var runtimeIdentifier = "";

			if (runtimeVariant == RuntimeVariant.NativeAOT)
			{
				buildProps.Add("PublishAot=true");
				buildProps.Add("PublishAotUsingRuntimePack=true"); // TODO: This parameter will become obsolete https://github.com/dotnet/runtime/issues/87060
				buildProps.Add("_IsPublishing=true"); // using dotnet build with -p:_IsPublishing=true enables targeting simulators
				buildProps.Add("IlcTreatWarningsAsErrors=false"); // TODO: Remove this once all warnings are fixed https://github.com/dotnet/maui/issues/19397
				// Restrict to iOS-only to avoid restoring NativeAOT packages for other platforms (e.g., Android)
				// which may not be available in the configured NuGet sources
				buildProps.Add($"TargetFrameworks={framework}-ios");
				// NativeAOT builds default to device (ios-arm64) when using PublishAot=true.
				// We must explicitly specify the simulator RID so the app can run on the simulator in our tests.
				runtimeIdentifier = TestEnvironment.IOSSimulatorRuntimeIdentifier;
			}

			if (!string.IsNullOrEmpty(trimMode))
			{
				buildProps.Add($"TrimMode={trimMode}");
				buildProps.Add("TrimmerSingleWarn=false"); // Disable trimmer warnings for iOS full trimming builds due to ObjCRuntime issues
			}

			Assert.IsTrue(DotnetInternal.Build(projectFile, config, framework: $"{framework}-ios", properties: buildProps, runtimeIdentifier: runtimeIdentifier),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

			// Find the .app bundle - it may be in the bin folder with or without a RID subfolder depending on build settings
			var appName = $"{Path.GetFileName(projectDir)}.app";
			var binDir = Path.Combine(projectDir, "bin", config, $"{framework}-ios");
			var appFile = Directory.GetDirectories(binDir, appName, SearchOption.AllDirectories).FirstOrDefault()
				?? Path.Combine(binDir, appName);

			// Write xh-results to the log directory for artifact collection
			var xhResultsDir = Path.Combine(TestEnvironment.GetLogDirectory(), "xh-results", Path.GetFileName(projectDir));
			Directory.CreateDirectory(xhResultsDir);

			// Let XHarness find the simulator based on target (e.g., ios-simulator-64_18.5).
			// Don't pass a specific UDID - this gives XHarness full control over the simulator
			// lifecycle and avoids race conditions with watchdog disabling.
			Assert.IsTrue(XHarness.RunAppleForTimeout(appFile, xhResultsDir, TestSimulator.XHarnessID),
				$"Project {Path.GetFileName(projectFile)} failed to run. Check test output/attachments for errors.");
		}
	}
}
