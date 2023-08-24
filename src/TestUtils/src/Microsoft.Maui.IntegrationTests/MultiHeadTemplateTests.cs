namespace Microsoft.Maui.IntegrationTests
{
	public class MultiHeadTemplateTests : BaseBuildTest
	{
		[Test]
		// Parameters: short name, target framework, build config
		[TestCase("maui-multihead", "Debug")]
		[TestCase("maui-multihead", "Release")]
		public void BuildAndroid(string id, string config)
		{
			var projectDir = TestDirectory;
			var appName = Path.GetFileName(projectDir);
			var projectFile = Path.Combine(projectDir, $"{appName}.Droid", $"{appName}.Droid.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir),
				$"Unable to create template {id}. Check test output for errors.");

			Assert.IsTrue(DotnetInternal.Build(projectFile, config),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
		}

		[Test]
		// Parameters: short name, target framework, build config
		[TestCase("maui-multihead", "Debug")]
		[TestCase("maui-multihead", "Release")]
		public void BuildiOS(string id, string config)
		{
			var projectDir = TestDirectory;
			var appName = Path.GetFileName(projectDir);
			var projectFile = Path.Combine(projectDir, $"{appName}.iOS", $"{appName}.iOS.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir),
				$"Unable to create template {id}. Check test output for errors.");

			Assert.IsTrue(DotnetInternal.Build(projectFile, config),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
		}

		[Test]
		// Parameters: short name, target framework, build config
		[TestCase("maui-multihead", "Debug")]
		[TestCase("maui-multihead", "Release")]
		public void BuildMacCatalyst(string id, string config)
		{
			var projectDir = TestDirectory;
			var appName = Path.GetFileName(projectDir);
			var projectFile = Path.Combine(projectDir, $"{appName}.MacCatalyst", $"{appName}.MacCatalyst.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir),
				$"Unable to create template {id}. Check test output for errors.");

			Assert.IsTrue(DotnetInternal.Build(projectFile, config),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
		}

		[Test]
		// Parameters: short name, target framework, build config
		[TestCase("maui-multihead", "Debug")]
		[TestCase("maui-multihead", "Release")]
		public void BuildWindows(string id, string config)
		{
			if (!TestEnvironment.IsWindows)
				Assert.Ignore("Building WinUI is only supported on Windows.");

			var projectDir = TestDirectory;
			var appName = Path.GetFileName(projectDir);
			var projectFile = Path.Combine(projectDir, $"{appName}.WinUI", $"{appName}.WinUI.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir),
				$"Unable to create template {id}. Check test output for errors.");

			Assert.IsTrue(DotnetInternal.Build(projectFile, config),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
		}
	}
}
