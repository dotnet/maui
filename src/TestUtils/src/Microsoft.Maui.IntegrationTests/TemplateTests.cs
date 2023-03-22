
namespace Microsoft.Maui.IntegrationTests
{
	public class TemplateTests : BaseBuildTest
	{
		[SetUp]
		public void TemplateTestsSetUp()
		{
			File.Copy(Path.Combine(TestEnvironment.GetMauiDirectory(), "src", "Templates", "tests", "Directory.Build.props"),
				Path.Combine(TestDirectory, "Directory.Build.props"), true);
			File.Copy(Path.Combine(TestEnvironment.GetMauiDirectory(), "src", "Templates", "tests", "Directory.Build.targets"),
				Path.Combine(TestDirectory, "Directory.Build.targets"), true);
		}

		[Test]
		// Parameters: short name, target framework, build config, use pack target
		[TestCase("maui", "net6.0", "Debug", false)]
		[TestCase("maui", "net6.0", "Release", false)]
		[TestCase("maui", "net7.0", "Debug", false)]
		[TestCase("maui", "net7.0", "Release", false)]
		[TestCase("maui-blazor", "net6.0", "Debug", false)]
		[TestCase("maui-blazor", "net6.0", "Release", false)]
		[TestCase("maui-blazor", "net7.0", "Debug", false)]
		[TestCase("maui-blazor", "net7.0", "Release", false)]
		[TestCase("mauilib", "net6.0", "Debug", true)]
		[TestCase("mauilib", "net6.0", "Release", true)]
		[TestCase("mauilib", "net7.0", "Debug", true)]
		[TestCase("mauilib", "net7.0", "Release", true)]
		public void Build(string id, string framework, string config, bool shouldPack)
		{
			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
				$"Unable to create template {id}. Check test output for errors.");

			EnableTizen(projectFile);

			string target = shouldPack ? "Pack" : "";
			Assert.IsTrue(DotnetInternal.Build(projectFile, config, target: target, properties: BuildProps),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
		}

		[Test]
		[TestCase("maui", "net6.0", "Debug")]
		[TestCase("maui", "net6.0", "Release")]
		[TestCase("maui", "net7.0", "Debug")]
		[TestCase("maui", "net7.0", "Release")]
		[TestCase("maui-blazor", "net6.0", "Debug")]
		[TestCase("maui-blazor", "net6.0", "Release")]
		[TestCase("maui-blazor", "net7.0", "Debug")]
		[TestCase("maui-blazor", "net7.0", "Release")]
		public void BuildUnpackaged(string id, string framework, string config)
		{
			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
				$"Unable to create template {id}. Check test output for errors.");

			EnableTizen(projectFile);
			FileUtilities.ReplaceInFile(projectFile,
				"<UseMaui>true</UseMaui>",
				"<UseMaui>true</UseMaui><WindowsPackageType>None</WindowsPackageType>");

			Assert.IsTrue(DotnetInternal.Build(projectFile, config, properties: BuildProps),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
		}

		[Test]
		[TestCase("mauilib", "net6.0", "Debug")]
		[TestCase("mauilib", "net6.0", "Release")]
		[TestCase("mauilib", "net7.0", "Debug")]
		[TestCase("mauilib", "net7.0", "Release")]
		public void PackCoreLib(string id, string framework, string config)
		{
			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
				$"Unable to create template {id}. Check test output for errors.");

			EnableTizen(projectFile);
			FileUtilities.ReplaceInFile(projectFile, new Dictionary<string, string>()
			{
				{ "UseMaui", "UseMauiCore" },
				{ "SingleProject", "EnablePreviewMsixTooling" },
			});
			Directory.Delete(Path.Combine(projectDir, "Platforms"), recursive: true);

			Assert.IsTrue(DotnetInternal.Build(projectFile, config, properties: BuildProps),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
		}

		void EnableTizen(string projectFile)
		{
			FileUtilities.ReplaceInFile(projectFile, new Dictionary<string, string>()
			{
				{ "<!-- <TargetFrameworks>", "<TargetFrameworks>" },
				{ "</TargetFrameworks> -->", "</TargetFrameworks>" },
			});
		}

	}
}
