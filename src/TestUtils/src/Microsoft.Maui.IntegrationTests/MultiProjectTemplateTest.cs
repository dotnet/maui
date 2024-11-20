namespace Microsoft.Maui.IntegrationTests;

[Category(Categories.MultiProject)]
public class MultiProjectTemplateTest : BaseTemplateTests
{
	[Test]
	[TestCase("Debug", "simplemulti")]
	[TestCase("Release", "simplemulti")]
	[TestCase("Debug", "MultiProject@Symbol & More")]
	[TestCase("Release", "MultiProject@Symbol & More")]
	public void BuildMultiProject(string config, string projectName)
	{
		var projectDir = Path.Combine(TestDirectory, projectName);
		var name = Path.GetFileName(projectDir);
		var solutionFile = Path.Combine(projectDir, $"{name}.sln");

		Assert.IsTrue(DotnetInternal.New("maui-multiproject", projectDir, DotNetCurrent),
			$"Unable to create template maui-multiproject. Check test output for errors.");

		if (!TestEnvironment.IsWindows)
		{
			Assert.IsTrue(DotnetInternal.Run("sln", $"\"{solutionFile}\" remove \"{projectDir}/{name}.WinUI/{name}.WinUI.csproj\""),
				$"Unable to remove WinUI project from solution. Check test output for errors.");
		}

		Assert.IsTrue(DotnetInternal.Build(solutionFile, config, properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Solution {name} failed to build. Check test output/attachments for errors.");
	}

	[Test]
	[TestCase("Debug", "--android")]
	[TestCase("Debug", "--ios")]
	[TestCase("Debug", "--windows")]
	[TestCase("Debug", "--macos")]
	public void BuildMultiProjectSinglePlatform(string config, string platformArg)
	{
		var projectDir = TestDirectory;
		var name = Path.GetFileName(projectDir);
		var solutionFile = Path.Combine(projectDir, $"{name}.sln");

		Assert.IsTrue(DotnetInternal.New($"maui-multiproject {platformArg}", projectDir, DotNetCurrent),
			$"Unable to create template maui-multiproject. Check test output for errors.");

		if (!TestEnvironment.IsWindows)
		{
			Assert.IsTrue(DotnetInternal.Run("sln", $"{solutionFile} remove {projectDir}/{name}.WinUI/{name}.WinUI.csproj"),
				$"Unable to remove WinUI project from solution. Check test output for errors.");
		}

		Assert.IsTrue(DotnetInternal.Build(solutionFile, config, properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Solution {name} failed to build. Check test output/attachments for errors.");
	}
}
