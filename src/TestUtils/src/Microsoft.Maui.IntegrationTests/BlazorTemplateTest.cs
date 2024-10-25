namespace Microsoft.Maui.IntegrationTests;

[Category(Categories.Blazor)]
public class BlazorTemplateTest : BaseTemplateTests
{
	[Test]
	// Parameters:  target framework, build config, dotnet new additional parameters

	// First, default scenarios
	[TestCase(DotNetCurrent, "Debug", "", false)]
	[TestCase(DotNetCurrent, "Release", "", false)]

	// Then, scenarios with additional template parameters:
	// - Interactivity Location: None/WASM/Server/Auto
	// - Empty vs. With Sample Content
	// - ProgramMain vs. TopLevel statements
	// And alternately testing other options for a healthy mix.
	[TestCase(DotNetCurrent, "Debug", "-I None --Empty", false)]
	[TestCase(DotNetCurrent, "Release", "-I WebAssembly --Empty", false)]
	[TestCase(DotNetCurrent, "Debug", "-I Server --Empty", false)]
	[TestCase(DotNetCurrent, "Release", "-I Auto --Empty", false)]
	[TestCase(DotNetCurrent, "Debug", "-I None", false)]
	[TestCase(DotNetCurrent, "Release", "-I WebAssembly", false)]
	[TestCase(DotNetCurrent, "Debug", "-I Server", false)]
	[TestCase(DotNetCurrent, "Release", "-I Auto", false)]
	[TestCase(DotNetCurrent, "Debug", "-I None --Empty --UseProgramMain", false)]
	[TestCase(DotNetCurrent, "Release", "-I WebAssembly --Empty --UseProgramMain", false)]
	[TestCase(DotNetCurrent, "Debug", "-I Server --Empty --UseProgramMain", false)]
	[TestCase(DotNetCurrent, "Release", "-I Auto --Empty --UseProgramMain", false)]
	[TestCase(DotNetCurrent, "Debug", "-I None --UseProgramMain", false)]
	[TestCase(DotNetCurrent, "Release", "-I WebAssembly --UseProgramMain", false)]
	[TestCase(DotNetCurrent, "Debug", "-I Server --UseProgramMain", false)]
	[TestCase(DotNetCurrent, "Release", "-I Auto --UseProgramMain", false)]

	// Then, some scenarios with tricky names in Debug builds only
	// This doesn't work on Android in Release, so we skip that for now
	// See https://github.com/dotnet/android/issues/9107
	[TestCase(DotNetCurrent, "Debug", "", true)]
	[TestCase(DotNetCurrent, "Debug", "-I Server --UseProgramMain", true)]
	public void BuildMauiBlazorWebSolution(string framework, string config, string additionalDotNetNewParams, bool useTrickyProjectName)
	{
		const string templateShortName = "maui-blazor-web";

		var solutionProjectDir = TestDirectory;
		if (useTrickyProjectName)
		{
			solutionProjectDir += "&More";
		}

		var webAppProjectDir = Path.Combine(solutionProjectDir, Path.GetFileName(solutionProjectDir) + ".Web");
		var webAppProjectFile = Path.Combine(webAppProjectDir, $"{Path.GetFileName(webAppProjectDir)}.csproj");

		var mauiAppProjectDir = Path.Combine(solutionProjectDir, Path.GetFileName(solutionProjectDir));
		var mauiAppProjectFile = Path.Combine(mauiAppProjectDir, $"{Path.GetFileName(mauiAppProjectDir)}.csproj");

		TestContext.WriteLine($"Creating project in {solutionProjectDir}");

		Assert.IsTrue(DotnetInternal.New(templateShortName, outputDirectory: solutionProjectDir, framework: framework, additionalDotNetNewParams: additionalDotNetNewParams),
			$"Unable to create template {templateShortName}. Check test output for errors.");

		TestContext.WriteLine($"Solution directory: {solutionProjectDir} (exists? {Directory.Exists(solutionProjectDir)})");
		TestContext.WriteLine($"Blazor Web app project directory: {webAppProjectDir} (exists? {Directory.Exists(webAppProjectDir)})");
		TestContext.WriteLine($"Blazor Web app project file: {webAppProjectFile} (exists? {File.Exists(webAppProjectFile)})");
		TestContext.WriteLine($"MAUI app project directory: {mauiAppProjectDir} (exists? {Directory.Exists(mauiAppProjectDir)})");
		TestContext.WriteLine($"MAUI app project file: {mauiAppProjectFile} (exists? {File.Exists(mauiAppProjectFile)})");

		EnableTizen(mauiAppProjectFile);

		TestContext.WriteLine($"Building Blazor Web app: {webAppProjectFile}");
		Assert.IsTrue(DotnetInternal.Build(webAppProjectFile, config, target: "", properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(webAppProjectFile)} failed to build. Check test output/attachments for errors.");

		TestContext.WriteLine($"Building .NET MAUI app: {mauiAppProjectFile}");
		Assert.IsTrue(DotnetInternal.Build(mauiAppProjectFile, config, target: "", properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(mauiAppProjectFile)} failed to build. Check test output/attachments for errors.");
	}
}
