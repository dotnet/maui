namespace Microsoft.Maui.IntegrationTests;

[Category(Categories.Blazor)]
public class BlazorTemplateTest : BaseTemplateTests
{
	[Test]
	// Parameters:  target framework, build config, dotnet new additional parameters, use tricky project name, additional dotnet build parameters

	// First, default scenarios
	[TestCase(DotNetCurrent, "Debug", "", false, "")]
	[TestCase(DotNetCurrent, "Release", "", false, "TrimMode=partial")]
	//[TestCase(DotNetCurrent, "Release", "", false, "TrimMode=full")]

	// Then, scenarios with additional template parameters:
	// - Interactivity Location: None/WASM/Server/Auto
	// - Empty vs. With Sample Content
	// - ProgramMain vs. TopLevel statements
	// And alternately testing other options for a healthy mix.
	// additionalDotNetBuildParams is a space-separated list of additional properties to pass to .NET MAUI build
	// Like "TrimMode=partial" for faster builds with AOT
	[TestCase(DotNetCurrent, "Debug", "--interactivity None --empty", false, "")]
	[TestCase(DotNetCurrent, "Release", "--interactivity WebAssembly --empty", false, "TrimMode=partial")]
	[TestCase(DotNetCurrent, "Debug", "--interactivity Server --empty", false, "")]
	[TestCase(DotNetCurrent, "Release", "--interactivity Auto --empty", false, "TrimMode=partial")]
	[TestCase(DotNetCurrent, "Debug", "--interactivity None", false, "")]
	[TestCase(DotNetCurrent, "Release", "--interactivity WebAssembly", false, "TrimMode=partial")]
	[TestCase(DotNetCurrent, "Debug", "--interactivity Server", false, "")]
	[TestCase(DotNetCurrent, "Release", "--interactivity Auto", false, "TrimMode=partial")]
	[TestCase(DotNetCurrent, "Debug", "--interactivity None --empty --use-program-main", false, "")]
	[TestCase(DotNetCurrent, "Release", "--interactivity WebAssembly --empty --use-program-main", false, "TrimMode=partial")]
	[TestCase(DotNetCurrent, "Debug", "--interactivity Server --empty --use-program-main", false, "")]
	[TestCase(DotNetCurrent, "Release", "--interactivity Auto --empty --use-program-main", false, "TrimMode=partial")]
	[TestCase(DotNetCurrent, "Debug", "--interactivity None --use-program-main", false, "")]
	[TestCase(DotNetCurrent, "Release", "--interactivity WebAssembly --use-program-main", false, "TrimMode=partial")]
	[TestCase(DotNetCurrent, "Debug", "--interactivity Server --use-program-main", false, "")]
	[TestCase(DotNetCurrent, "Release", "--interactivity Auto --use-program-main", false, "TrimMode=partial")]

	// Then, some scenarios with tricky names in Debug builds only
	// This doesn't work on Android in Release, so we skip that for now
	// See https://github.com/dotnet/android/issues/9107
	// [TestCase(DotNetCurrent, "Debug", "", true)]
	// [TestCase(DotNetCurrent, "Debug", "--interactivity Server --use-program-main", true)]
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

		TestContext.WriteLine($"Creating project in {solutionProjectDir}");

		Assert.IsTrue(DotnetInternal.New(templateShortName, outputDirectory: solutionProjectDir, framework: framework, additionalDotNetNewParams: additionalDotNetNewParams),
			$"Unable to create template {templateShortName}. Check test output for errors.");

		TestContext.WriteLine($"Solution directory: {solutionProjectDir} (exists? {Directory.Exists(solutionProjectDir)})");
		TestContext.WriteLine($"Blazor Web app project directory: {webAppProjectDir} (exists? {Directory.Exists(webAppProjectDir)})");
		TestContext.WriteLine($"Blazor Web app project file: {webAppProjectFile} (exists? {File.Exists(webAppProjectFile)})");
		TestContext.WriteLine($"MAUI app project directory: {mauiAppProjectDir} (exists? {Directory.Exists(mauiAppProjectDir)})");
		TestContext.WriteLine($"MAUI app project file: {mauiAppProjectFile} (exists? {File.Exists(mauiAppProjectFile)})");

		TestContext.WriteLine($"Building Blazor Web app: {webAppProjectFile}");
		Assert.IsTrue(DotnetInternal.Build(webAppProjectFile, config, target: "", properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(webAppProjectFile)} failed to build. Check test output/attachments for errors.");

		TestContext.WriteLine($"Building .NET MAUI app: {mauiAppProjectFile} props: {buildProps}");
		Assert.IsTrue(DotnetInternal.Build(mauiAppProjectFile, config, target: "", properties: buildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(mauiAppProjectFile)} failed to build. Check test output/attachments for errors.");
	}
}
