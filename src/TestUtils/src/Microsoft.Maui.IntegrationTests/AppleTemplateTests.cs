
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
		[TestCase("maui", "Debug", "net6.0")]
		[TestCase("maui", "Release", "net6.0")]
		[TestCase("maui", "Debug", "net7.0")]
		[TestCase("maui", "Release", "net7.0")]
		[TestCase("maui-blazor", "Debug", "net6.0")]
		[TestCase("maui-blazor", "Release", "net6.0")]
		[TestCase("maui-blazor", "Debug", "net7.0")]
		[TestCase("maui-blazor", "Release", "net7.0")]
		public void RunOniOS(string id, string config, string framework)
		{
			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
				$"Unable to create template {id}. Check test output for errors.");

			Assert.IsTrue(DotnetInternal.Build(projectFile, config, framework: $"{framework}-ios", properties: BuildProps),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

			var appFile = Path.Combine(projectDir, "bin", config, $"{framework}-ios", "iossimulator-x64", $"{Path.GetFileName(projectDir)}.app");

			Assert.IsTrue(XHarness.RunAppleForTimeout(appFile, Path.Combine(projectDir, "xh-results"), TestSimulator.XHarnessID, 25),
				$"Project {Path.GetFileName(projectFile)} failed to run. Check test output/attachments for errors.");
		}

	}
}
