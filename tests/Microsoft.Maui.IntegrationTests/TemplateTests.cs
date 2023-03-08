
namespace Microsoft.Maui.IntegrationTests
{
	public class TemplateTests : BaseBuildTest
	{
		[Test]
		// Parameters: short name, build config, target framework, use pack target
		[TestCase("maui", "Debug", "net7.0", false)]
		[TestCase("maui", "Release", "net7.0", false)]
		[TestCase("maui-blazor", "Debug", "net7.0", false)]
		[TestCase("maui-blazor", "Release", "net7.0", false)]
		[TestCase("mauilib", "Debug", "net7.0", true)]
		[TestCase("mauilib", "Release", "net7.0", true)]
		public void Build(string id, string config, string framework, bool shouldPack)
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
		// Parameters: short name, build config, target framework
		[TestCase("maui", "Debug", "net7.0")]
		[TestCase("maui", "Release", "net7.0")]
		[TestCase("maui-blazor", "Debug", "net7.0")]
		[TestCase("maui-blazor", "Release", "net7.0")]
		public void BuildUnpackaged(string id, string config, string framework)
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
		[TestCase("Debug", "net7.0")]
		[TestCase("Release", "net7.0")]
		public void PackCoreLib(string config, string framework)
		{
			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.IsTrue(DotnetInternal.New("mauilib", projectDir, framework),
				$"Unable to create template mauilib. Check test output for errors.");

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