
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

		[Test]
		[TestCase("maui", "Debug", DotNetPrevious)]
		[TestCase("maui", "Release", DotNetPrevious)]
		[TestCase("maui", "Debug", DotNetCurrent)]
		[TestCase("maui", "Release", DotNetCurrent)]
		[TestCase("maui-blazor", "Debug", DotNetPrevious)]
		[TestCase("maui-blazor", "Release", DotNetPrevious)]
		[TestCase("maui-blazor", "Debug", DotNetCurrent)]
		[TestCase("maui-blazor", "Release", DotNetCurrent)]
		public void RunOniOS(string id, string config, string framework)
		{
			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
				$"Unable to create template {id}. Check test output for errors.");

			Assert.IsTrue(DotnetInternal.Build(projectFile, config, framework: $"{framework}-ios", properties: BuildProps),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

			var appFile = Path.Combine(projectDir, "bin", config, $"{framework}-ios", "iossimulator-x64", $"{Path.GetFileName(projectDir)}.app");

			Assert.IsTrue(XHarness.RunAppleForTimeout(appFile, Path.Combine(projectDir, "xh-results"), TestSimulator.XHarnessID),
				$"Project {Path.GetFileName(projectFile)} failed to run. Check test output/attachments for errors.");
		}

		[Test]
		[TestCase("maui", "Release", DotNetCurrent, "iossimulator-x64")]
		public void RunOniOSNativeAOT(string id, string config, string framework, string runtimeIdentifier)
		{
			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
				$"Unable to create template {id}. Check test output for errors.");

			var extendedBuildProps = BuildProps;
			extendedBuildProps.Add("PublishAot=true");
			extendedBuildProps.Add("PublishAotUsingRuntimePack=true"); // TODO: This parameter will become obsolete https://github.com/dotnet/runtime/issues/87060
			extendedBuildProps.Add("_IsPublishing=true"); // using dotnet build with -p:_IsPublishing=true enables targeting simulators
			extendedBuildProps.Add($"RuntimeIdentifier={runtimeIdentifier}");

			Assert.IsTrue(DotnetInternal.Build(projectFile, config, framework: $"{framework}-ios", properties: BuildProps),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

			var appFile = Path.Combine(projectDir, "bin", config, $"{framework}-ios", runtimeIdentifier, $"{Path.GetFileName(projectDir)}.app");

			Assert.IsTrue(XHarness.RunAppleForTimeout(appFile, Path.Combine(projectDir, "xh-results"), TestSimulator.XHarnessID),
				$"Project {Path.GetFileName(projectFile)} failed to run. Check test output/attachments for errors.");
		}
	}
}
