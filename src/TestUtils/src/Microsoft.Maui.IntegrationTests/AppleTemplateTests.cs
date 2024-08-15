
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

			TestSimulator.Shutdown();
			Assert.IsTrue(TestSimulator.Launch(), $"Failed to boot simulator with UDID '{TestSimulator.GetUDID()}'.");
			TestSimulator.ShowWindow();
		}

		[TearDown]
		public void AppleTestTearDown()
		{
			TestSimulator.Log(Path.Combine(TestDirectory, "xh-results"));
		}

		[OneTimeTearDown]
		public void AppleTemplateFxtTearDown()
		{
			TestSimulator.Shutdown();
		}

		[Test]
		[TestCase("maui", "Debug", DotNetPrevious, "iossimulator-x64", RuntimeVariant.Mono, null)]
		[TestCase("maui", "Release", DotNetPrevious, "iossimulator-x64", RuntimeVariant.Mono, null)]
		[TestCase("maui", "Debug", DotNetCurrent, "iossimulator-x64", RuntimeVariant.Mono, null)]
		[TestCase("maui", "Release", DotNetCurrent, "iossimulator-x64", RuntimeVariant.Mono, null)]
		[TestCase("maui", "Release", DotNetCurrent, "iossimulator-x64", RuntimeVariant.Mono, "full")]
		[TestCase("maui-blazor", "Debug", DotNetPrevious, "iossimulator-x64", RuntimeVariant.Mono, null)]
		[TestCase("maui-blazor", "Release", DotNetPrevious, "iossimulator-x64", RuntimeVariant.Mono, null)]
		[TestCase("maui-blazor", "Debug", DotNetCurrent, "iossimulator-x64", RuntimeVariant.Mono, null)]
		[TestCase("maui-blazor", "Release", DotNetCurrent, "iossimulator-x64", RuntimeVariant.Mono, null)]
		// FIXME: has trimmer warnings
		//[TestCase("maui-blazor", "Release", DotNetCurrent, "iossimulator-x64", RuntimeVariant.Mono, "full")]
		//[TestCase("maui", "Release", DotNetCurrent, "iossimulator-x64", RuntimeVariant.NativeAOT, null)]
		public void RunOniOS(string id, string config, string framework, string runtimeIdentifier, RuntimeVariant runtimeVariant, string trimMode)
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
				buildProps.Add($"RuntimeIdentifier={runtimeIdentifier}");
				buildProps.Add("IlcTreatWarningsAsErrors=false"); // TODO: Remove this once all warnings are fixed https://github.com/dotnet/maui/issues/19397
			}

			if (!string.IsNullOrEmpty(trimMode))
			{
				buildProps.Add($"TrimMode={trimMode}");
				buildProps.Add("TrimmerSingleWarn=false");
			}

			Assert.IsTrue(DotnetInternal.Build(projectFile, config, framework: $"{framework}-ios", properties: buildProps),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

			var appFile = Path.Combine(projectDir, "bin", config, $"{framework}-ios", runtimeIdentifier, $"{Path.GetFileName(projectDir)}.app");

			Assert.IsTrue(XHarness.RunAppleForTimeout(appFile, Path.Combine(projectDir, "xh-results"), TestSimulator.XHarnessID),
				$"Project {Path.GetFileName(projectFile)} failed to run. Check test output/attachments for errors.");
		}
	}
}
