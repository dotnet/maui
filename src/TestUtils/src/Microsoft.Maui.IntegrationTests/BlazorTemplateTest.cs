namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Blazor")]
public class BlazorTemplateTest : BaseTemplateTests
{
	[Theory]
	// Parameters:  target framework, build config, dotnet new additional parameters, use tricky project name, additional dotnet build parameters

	// First, default scenarios
	[InlineData(DotNetCurrent, "Debug", "", false, "")]
	[InlineData(DotNetCurrent, "Release", "", false, "TrimMode=partial")]
	//[InlineData(DotNetCurrent, "Release", "", false, "TrimMode=full")]

	// Then, scenarios with additional template parameters:
	// - Interactivity Location: None/WASM/Server/Auto
	// - Empty vs. With Sample Content
	// - ProgramMain vs. TopLevel statements
	// And alternately testing other options for a healthy mix.
	// additionalDotNetBuildParams is a space-separated list of additional properties to pass to .NET MAUI build
	// Like "TrimMode=partial" for faster builds with AOT
	[InlineData(DotNetCurrent, "Debug", "-I None --Empty", false, "")]
	[InlineData(DotNetCurrent, "Release", "-I WebAssembly --Empty", false, "TrimMode=partial")]
	[InlineData(DotNetCurrent, "Debug", "-I Server --Empty", false, "")]
	[InlineData(DotNetCurrent, "Release", "-I Auto --Empty", false, "TrimMode=partial")]
	[InlineData(DotNetCurrent, "Debug", "-I None", false, "")]
	[InlineData(DotNetCurrent, "Release", "-I WebAssembly", false, "TrimMode=partial")]
	[InlineData(DotNetCurrent, "Debug", "-I Server", false, "")]
	[InlineData(DotNetCurrent, "Release", "-I Auto", false, "TrimMode=partial")]
	[InlineData(DotNetCurrent, "Debug", "-I None --Empty --UseProgramMain", false, "")]
	[InlineData(DotNetCurrent, "Release", "-I WebAssembly --Empty --UseProgramMain", false, "TrimMode=partial")]
	[InlineData(DotNetCurrent, "Debug", "-I Server --Empty --UseProgramMain", false, "")]
	[InlineData(DotNetCurrent, "Release", "-I Auto --Empty --UseProgramMain", false, "TrimMode=partial")]
	[InlineData(DotNetCurrent, "Debug", "-I None --UseProgramMain", false, "")]
	[InlineData(DotNetCurrent, "Release", "-I WebAssembly --UseProgramMain", false, "TrimMode=partial")]
	[InlineData(DotNetCurrent, "Debug", "-I Server --UseProgramMain", false, "")]
	[InlineData(DotNetCurrent, "Release", "-I Auto --UseProgramMain", false, "TrimMode=partial")]

	// Then, some scenarios with tricky names in Debug builds only
	// This doesn't work on Android in Release, so we skip that for now
	// See https://github.com/dotnet/android/issues/9107
	// [InlineData(DotNetCurrent, "Debug", "", true)]
	// [InlineData(DotNetCurrent, "Debug", "-I Server --UseProgramMain", true)]
	public void BuildMauiBlazorWebSolution(string framework, string config, string additionalDotNetNewParams, bool useTrickyProjectName, string additionalDotNetBuildParams)
	{
		const string templateShortName = "maui-blazor-web";

		var solutionProjectDir = TestDirectory;
		if (useTrickyProjectName)
		{
			solutionProjectDir += "&More";
		}

		var buildProps = BuildProps;

		if (additionalDotNetBuildParams is not "" and not null)
		{
			additionalDotNetBuildParams.Split(" ").ToList().ForEach(p => buildProps.Add(p));
		}

		var webAppProjectDir = Path.Combine(solutionProjectDir, Path.GetFileName(solutionProjectDir) + ".Web");
		var webAppProjectFile = Path.Combine(webAppProjectDir, $"{Path.GetFileName(webAppProjectDir)}.csproj");

		var mauiAppProjectDir = Path.Combine(solutionProjectDir, Path.GetFileName(solutionProjectDir));
		var mauiAppProjectFile = Path.Combine(mauiAppProjectDir, $"{Path.GetFileName(mauiAppProjectDir)}.csproj");

// 		TestContext.WriteLine($"Creating project in {solutionProjectDir}");

		Assert.True(DotnetInternal.New(templateShortName, outputDirectory: solutionProjectDir, framework: framework, additionalDotNetNewParams: additionalDotNetNewParams),
			$"Unable to create template {templateShortName}. Check test output for errors.");

// 		TestContext.WriteLine($"Solution directory: {solutionProjectDir} (exists? {Directory.Exists(solutionProjectDir)})");
// 		TestContext.WriteLine($"Blazor Web app project directory: {webAppProjectDir} (exists? {Directory.Exists(webAppProjectDir)})");
// 		TestContext.WriteLine($"Blazor Web app project file: {webAppProjectFile} (exists? {File.Exists(webAppProjectFile)})");
// 		TestContext.WriteLine($"MAUI app project directory: {mauiAppProjectDir} (exists? {Directory.Exists(mauiAppProjectDir)})");
// 		TestContext.WriteLine($"MAUI app project file: {mauiAppProjectFile} (exists? {File.Exists(mauiAppProjectFile)})");

		EnableTizen(mauiAppProjectFile);

// 		TestContext.WriteLine($"Building Blazor Web app: {webAppProjectFile}");
		Assert.True(DotnetInternal.Build(webAppProjectFile, config, target: "", properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(webAppProjectFile)} failed to build. Check test output/attachments for errors.");

// 		TestContext.WriteLine($"Building .NET MAUI app: {mauiAppProjectFile} props: {buildProps}");
		Assert.True(DotnetInternal.Build(mauiAppProjectFile, config, target: "", properties: buildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(mauiAppProjectFile)} failed to build. Check test output/attachments for errors.");
	}
}
