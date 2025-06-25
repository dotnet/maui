
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
			Assert.True(TestSimulator.Launch(), $"Failed to boot simulator with UDID '{TestSimulator.GetUDID()}'.");
			TestSimulator.ShowWindow();
		}

		[OneTimeTearDown]
		public void AppleTemplateFxtTearDown()
		{
			TestSimulator.Shutdown();
		}

		[Fact]
		// [Theory]
		[InlineData("maui", "Debug", DotNetPrevious, "iossimulator-x64", RuntimeVariant.Mono, null)]
		// [Theory]
		[InlineData("maui", "Release", DotNetPrevious, "iossimulator-x64", RuntimeVariant.Mono, null)]
		[Theory]
		[InlineData("maui", "Debug", DotNetCurrent, "iossimulator-x64", RuntimeVariant.Mono, null)]
		[Theory]
		[InlineData("maui", "Release", DotNetCurrent, "iossimulator-x64", RuntimeVariant.Mono, null)]
		[Theory]
		[InlineData("maui", "Release", DotNetCurrent, "iossimulator-x64", RuntimeVariant.Mono, "full")]
		// [Theory]
		[InlineData("maui-blazor", "Debug", DotNetPrevious, "iossimulator-x64", RuntimeVariant.Mono, null)]
		// [Theory]
		[InlineData("maui-blazor", "Release", DotNetPrevious, "iossimulator-x64", RuntimeVariant.Mono, null)]
		[Theory]
		[InlineData("maui-blazor", "Debug", DotNetCurrent, "iossimulator-x64", RuntimeVariant.Mono, null)]
		[Theory]
		[InlineData("maui-blazor", "Release", DotNetCurrent, "iossimulator-x64", RuntimeVariant.Mono, null)]
		// FIXME: has trimmer warnings
		//[Theory]
		[InlineData("maui-blazor", "Release", DotNetCurrent, "iossimulator-x64", RuntimeVariant.Mono, "full")]
		[Theory]
		[InlineData("maui", "Release", DotNetCurrent, "iossimulator-x64", RuntimeVariant.NativeAOT, null)]
		public void RunOniOS(string id, string config, string framework, string runtimeIdentifier, RuntimeVariant runtimeVariant, string trimMode)
		{
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
				buildProps.Add($"RuntimeIdentifier={runtimeIdentifier}");
				buildProps.Add("IlcTreatWarningsAsErrors=false"); // TODO: Remove this once all warnings are fixed https://github.com/dotnet/maui/issues/19397
			}

			if (!string.IsNullOrEmpty(trimMode))
			{
				buildProps.Add($"TrimMode={trimMode}");
				buildProps.Add("TrimmerSingleWarn=false");
			}

			Assert.True(DotnetInternal.Build(projectFile, config, framework: $"{framework}-ios", properties: buildProps),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

			var appFile = Path.Combine(projectDir, "bin", config, $"{framework}-ios", runtimeIdentifier, $"{Path.GetFileName(projectDir)}.app");

			Assert.True(XHarness.RunAppleForTimeout(appFile, Path.Combine(projectDir, "xh-results"), TestSimulator.XHarnessID),
				$"Project {Path.GetFileName(projectFile)} failed to run. Check test output/attachments for errors.");
		}
	}
}
