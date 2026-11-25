namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "MultiProject")]
public class MultiProjectTemplateTest : BaseTemplateTests
{
	public MultiProjectTemplateTest(IntegrationTestFixture fixture, ITestOutputHelper output) : base(fixture, output) { }

	[Fact]
	public void NewSolutionIncludesGitIgnore()
	{
		SetTestIdentifier();
		var projectDir = TestDirectory;

		Assert.True(DotnetInternal.New("maui-multiproject", projectDir, DotNetCurrent, output: _output),
			$"Unable to create template maui-multiproject. Check test output for errors.");

		AssertIncludesRootGitIgnore(projectDir);
	}

	[Theory]
	[InlineData("Debug", "simplemulti")]
	[InlineData("Release", "simplemulti")]
	[InlineData("Debug", "MultiProject@Symbol & More")]
	[InlineData("Release", "MultiProject@Symbol & More")]
	public void BuildMultiProject(string config, string projectName)
	{
		SetTestIdentifier(config, projectName);
		var projectDir = Path.Combine(TestDirectory, projectName);
		var name = Path.GetFileName(projectDir);
		var solutionFile = Path.Combine(projectDir, $"{name}.sln");

		Assert.True(DotnetInternal.New("maui-multiproject", projectDir, DotNetCurrent, output: _output),
			$"Unable to create template maui-multiproject. Check test output for errors.");

		// Always remove WinUI project if the project name contains special characters that cause WinRT source generator issues
		// See: https://github.com/microsoft/CsWinRT/issues/1809 (under "Special characters in assembly name" section)
		bool containsSpecialChars = projectName.IndexOfAny(new[] { '@', '&', '+', '%', '!', '#', '$', '^', '*', ' ', '-' }) >= 0;

		if (!TestEnvironment.IsWindows || containsSpecialChars)
		{
			Assert.True(DotnetInternal.Run("sln", $"\"{solutionFile}\" remove \"{projectDir}/{name}.WinUI/{name}.WinUI.csproj\"", output: _output),
				$"Unable to remove WinUI project from solution. Check test output for errors.");
		}

		// TODO, we should not need this but hitting: https://github.com/dotnet/maui/issues/19840
		var buildProps = BuildProps;
		buildProps.Add("ResizetizerErrorOnDuplicateOutputFilename=false");

		Assert.True(DotnetInternal.Build(solutionFile, config, properties: buildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Solution {name} failed to build. Check test output/attachments for errors.");
	}

	[Theory]
	[InlineData("Debug", "--android")]
	[InlineData("Debug", "--ios")]
	[InlineData("Debug", "--windows")]
	[InlineData("Debug", "--macos")]
	public void BuildMultiProjectSinglePlatform(string config, string platformArg)
	{
		SetTestIdentifier(config, platformArg);
		var projectDir = TestDirectory;
		var name = Path.GetFileName(projectDir);
		var solutionFile = Path.Combine(projectDir, $"{name}.sln");

		Assert.True(DotnetInternal.New($"maui-multiproject {platformArg}", projectDir, DotNetCurrent, output: _output),
			$"Unable to create template maui-multiproject. Check test output for errors.");

		if (!TestEnvironment.IsWindows)
		{
			Assert.True(DotnetInternal.Run("sln", $"{solutionFile} remove {projectDir}/{name}.WinUI/{name}.WinUI.csproj", output: _output),
				$"Unable to remove WinUI project from solution. Check test output for errors.");
		}

		Assert.True(DotnetInternal.Build(solutionFile, config, properties: BuildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Solution {name} failed to build. Check test output/attachments for errors.");
	}

	[Theory]
	[InlineData("Debug", Skip = AvaloniaBuildSkipReason)]
	[InlineData("Release", Skip = AvaloniaBuildSkipReason)]
	public void BuildMultiProjectWithAvalonia(string config)
	{
		SetTestIdentifier(config);
		var projectDir = TestDirectory;
		var name = Path.GetFileName(projectDir);
		var solutionFile = Path.Combine(projectDir, $"{name}.sln");

		// --avalonia on its own produces the Avalonia desktop head and no native heads.
		Assert.True(DotnetInternal.New("maui-multiproject --avalonia", projectDir, DotNetCurrent, output: _output),
			$"Unable to create template maui-multiproject. Check test output for errors.");

		var buildProps = BuildProps;
		buildProps.RemoveAll(p => p.StartsWith("RestoreConfigFile=", StringComparison.Ordinal));
		buildProps.Add($"RestoreConfigFile={CreateAvaloniaNuGetConfig(projectDir)}");

		Assert.True(DotnetInternal.Build(solutionFile, config, properties: buildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Solution {name} failed to build. Check test output/attachments for errors.");
	}

	[Theory]
	[InlineData("Debug", "--android", Skip = AvaloniaBuildSkipReason)]
	[InlineData("Debug", "--macos", Skip = AvaloniaBuildSkipReason)]
	public void BuildMultiProjectWithAvaloniaEmbedding(string config, string platformArg)
	{
		SetTestIdentifier(config, platformArg);
		var projectDir = TestDirectory;
		var name = Path.GetFileName(projectDir);
		var solutionFile = Path.Combine(projectDir, $"{name}.sln");

		// Combining --avalonia with a native head adds the desktop head and switches the
		// native head to UseAvaloniaEmbedding.
		Assert.True(DotnetInternal.New($"maui-multiproject {platformArg} --avalonia", projectDir, DotNetCurrent, output: _output),
			$"Unable to create template maui-multiproject. Check test output for errors.");

		var buildProps = BuildProps;
		buildProps.RemoveAll(p => p.StartsWith("RestoreConfigFile=", StringComparison.Ordinal));
		buildProps.Add($"RestoreConfigFile={CreateAvaloniaNuGetConfig(projectDir)}");

		Assert.True(DotnetInternal.Build(solutionFile, config, properties: buildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Solution {name} failed to build. Check test output/attachments for errors.");
	}

	[Theory]
	[InlineData("--android")]
	[InlineData("--ios")]
	[InlineData("--windows")]
	[InlineData("--macos")]
	[InlineData("--avalonia")]
	[InlineData("--android --avalonia")]
	[InlineData("--ios --avalonia")]
	[InlineData("--windows --avalonia")]
	[InlineData("--macos --avalonia")]
	[InlineData("")] // no platform arg means all platforms
					 // https://github.com/dotnet/maui/issues/28695
	public void VerifyIncludedPlatformsInSln(string platformArg)
	{
		SetTestIdentifier(platformArg);
		var projectDir = TestDirectory;
		var name = Path.GetFileName(projectDir);
		var solutionFile = Path.Combine(projectDir, $"{name}.sln");

		Assert.True(DotnetInternal.New($"maui-multiproject {platformArg}", projectDir, DotNetCurrent, output: _output),
			$"Unable to create template maui-multiproject. Check test output for errors.");

		var slnListOutput = DotnetInternal.RunForOutput("sln", $"{solutionFile} list", out int exitCode, output: _output);

		// Asserts the process completed successfully
		if (exitCode != 0)
			Assert.Fail($"Unable to list projects in solution. Check test output for errors.");

		// Asserts if the shared project is included in the solution, this should always be the case
		Assert.True(slnListOutput.Contains($"{name}.csproj", StringComparison.OrdinalIgnoreCase),
			$"Expected shared project (with name {name}.csproj) to be included in the solution.");

		var args = platformArg.Split(' ', StringSplitOptions.RemoveEmptyEntries);
		var avalonia = args.Contains("--avalonia");

		// The Avalonia desktop head is opt-in only, so it is absent from the no-platform-arg default.
		// Any platform arg (including --avalonia) switches from "all native heads" to "only the requested heads".
		var expectedHeads = new List<string>();
		if (args.Length == 0 || args.Contains("--android"))
			expectedHeads.Add("Droid");
		if (args.Length == 0 || args.Contains("--ios"))
			expectedHeads.Add("iOS");
		if (args.Length == 0 || args.Contains("--macos"))
			expectedHeads.Add("Mac");
		if (args.Length == 0 || args.Contains("--windows"))
			expectedHeads.Add("WinUI");
		if (avalonia)
			expectedHeads.Add("Desktop");

		// Match the full head project filename so a shared project name ending in a head suffix cannot produce a false positive.
		var allHeads = new[] { "Droid", "iOS", "Mac", "WinUI", "Desktop" };
		foreach (var head in allHeads.Except(expectedHeads))
		{
			Assert.False(slnListOutput.Contains($"{name}.{head}.csproj", StringComparison.Ordinal),
				$"Expected {name}.{head}.csproj to NOT be included in the solution.");
		}

		// Depending on the platform argument, we assert if the expected projects are included in the solution
		foreach (var head in expectedHeads)
		{
			Assert.True(slnListOutput.Contains($"{name}.{head}.csproj", StringComparison.Ordinal),
				$"Expected {name}.{head}.csproj to be included in the solution.");
		}

		// The template engine silently drops conditional blocks whose symbol is unknown, so check the
		// generated content of the shared project and every native head rather than just solution membership.
		var sharedCsproj = File.ReadAllText(Path.Combine(projectDir, name, $"{name}.csproj"));
		AssertAvaloniaEmbedding(avalonia, sharedCsproj, mauiProgram: null);

		foreach (var head in expectedHeads.Where(h => h != "Desktop"))
		{
			var headDir = Path.Combine(projectDir, $"{name}.{head}");
			var headCsproj = File.ReadAllText(Path.Combine(headDir, $"{name}.{head}.csproj"));
			var mauiProgram = File.ReadAllText(Path.Combine(headDir, "MauiProgram.cs"));
			AssertAvaloniaEmbedding(avalonia, headCsproj, mauiProgram);
		}

		if (avalonia)
		{
			var desktopDir = Path.Combine(projectDir, $"{name}.Desktop");
			AssertContains("Include=\"Avalonia.Controls.Maui.Desktop\"", File.ReadAllText(Path.Combine(desktopDir, $"{name}.Desktop.csproj")));
			AssertContains(".UseAvaloniaApp(", File.ReadAllText(Path.Combine(desktopDir, "MauiProgram.cs")));
		}
		else
		{
			Assert.False(Directory.Exists(Path.Combine(projectDir, $"{name}.Desktop")),
				$"Expected {name}.Desktop to NOT be generated without --avalonia.");
		}
	}

	void AssertAvaloniaEmbedding(bool expected, string csproj, string? mauiProgram)
	{
		if (expected)
		{
			AssertContains("Include=\"Avalonia.Controls.Maui\"", csproj);
			AssertContains("<AvaloniaControlsMauiGenerateBootstrap>", csproj);
			if (mauiProgram is not null)
				AssertContains(".UseAvaloniaEmbedding<AvaloniaApp>()", mauiProgram);
		}
		else
		{
			AssertDoesNotContain("Avalonia", csproj);
			if (mauiProgram is not null)
				AssertDoesNotContain("Avalonia", mauiProgram);
		}
	}
}
