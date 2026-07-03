using System.IO.Compression;

namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Blazor")]
public class BlazorTemplateTest : BaseTemplateTests
{
	public BlazorTemplateTest(IntegrationTestFixture fixture, ITestOutputHelper output) : base(fixture, output) { }

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
	[InlineData(DotNetCurrent, "Debug", "--interactivity None --empty", false, "")]
	[InlineData(DotNetCurrent, "Release", "--interactivity WebAssembly --empty", false, "TrimMode=partial")]
	[InlineData(DotNetCurrent, "Debug", "--interactivity Server --empty", false, "")]
	[InlineData(DotNetCurrent, "Release", "--interactivity Auto --empty", false, "TrimMode=partial")]
	[InlineData(DotNetCurrent, "Debug", "--interactivity None", false, "")]
	[InlineData(DotNetCurrent, "Release", "--interactivity WebAssembly", false, "TrimMode=partial")]
	[InlineData(DotNetCurrent, "Debug", "--interactivity Server", false, "")]
	[InlineData(DotNetCurrent, "Release", "--interactivity Auto", false, "TrimMode=partial")]
	[InlineData(DotNetCurrent, "Debug", "--interactivity None --empty --use-program-main", false, "")]
	[InlineData(DotNetCurrent, "Release", "--interactivity WebAssembly --empty --use-program-main", false, "TrimMode=partial")]
	[InlineData(DotNetCurrent, "Debug", "--interactivity Server --empty --use-program-main", false, "")]
	[InlineData(DotNetCurrent, "Release", "--interactivity Auto --empty --use-program-main", false, "TrimMode=partial")]
	[InlineData(DotNetCurrent, "Debug", "--interactivity None --use-program-main", false, "")]
	[InlineData(DotNetCurrent, "Release", "--interactivity WebAssembly --use-program-main", false, "TrimMode=partial")]
	[InlineData(DotNetCurrent, "Debug", "--interactivity Server --use-program-main", false, "")]
	[InlineData(DotNetCurrent, "Release", "--interactivity Auto --use-program-main", false, "TrimMode=partial")]

	// Then, some scenarios with tricky names in Debug builds only
	// This doesn't work on Android in Release, so we skip that for now
	// See https://github.com/dotnet/android/issues/9107
	// [InlineData(DotNetCurrent, "Debug", "", true)]
	// [InlineData(DotNetCurrent, "Debug", "--interactivity Server --use-program-main", true)]
	public void BuildMauiBlazorWebSolution(string framework, string config, string additionalDotNetNewParams, bool useTrickyProjectName, string additionalDotNetBuildParams)
	{
		SetTestIdentifier(framework, config, additionalDotNetNewParams, useTrickyProjectName, additionalDotNetBuildParams);
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

		_output.WriteLine($"Creating project in {solutionProjectDir}");

		Assert.True(DotnetInternal.New(templateShortName, outputDirectory: solutionProjectDir, framework: framework, additionalDotNetNewParams: additionalDotNetNewParams, output: _output),
			$"Unable to create template {templateShortName}. Check test output for errors.");

		_output.WriteLine($"Solution directory: {solutionProjectDir} (exists? {Directory.Exists(solutionProjectDir)})");
		_output.WriteLine($"Blazor Web app project directory: {webAppProjectDir} (exists? {Directory.Exists(webAppProjectDir)})");
		_output.WriteLine($"Blazor Web app project file: {webAppProjectFile} (exists? {File.Exists(webAppProjectFile)})");
		_output.WriteLine($"MAUI app project directory: {mauiAppProjectDir} (exists? {Directory.Exists(mauiAppProjectDir)})");
		_output.WriteLine($"MAUI app project file: {mauiAppProjectFile} (exists? {File.Exists(mauiAppProjectFile)})");

		_output.WriteLine($"Building Blazor Web app: {webAppProjectFile}");
		Assert.True(DotnetInternal.Build(webAppProjectFile, config, target: "", properties: BuildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Project {Path.GetFileName(webAppProjectFile)} failed to build. Check test output/attachments for errors.");

		_output.WriteLine($"Building .NET MAUI app: {mauiAppProjectFile} props: {buildProps}");
		Assert.True(DotnetInternal.Build(mauiAppProjectFile, config, target: "", properties: buildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Project {Path.GetFileName(mauiAppProjectFile)} failed to build. Check test output/attachments for errors.");
	}

	/// <summary>
	/// Regression test for https://github.com/dotnet/maui/issues/33773
	/// Verifies that MAUI Blazor Hybrid apps with shared RCLs do not include
	/// precompressed .gz/.br files in the Android APK, as these bloat the app
	/// bundle unnecessarily (assets are served locally, not over HTTP).
	/// </summary>
	[Fact]
	public void MauiBlazorWebApk_DoesNotContainCompressedRclAssets()
	{
		SetTestIdentifier("MauiBlazorWebApk_NoCompression");
		const string templateShortName = "maui-blazor-web";
		const string config = "Release";

		var solutionProjectDir = TestDirectory;
		var solutionName = Path.GetFileName(solutionProjectDir);

		// Project paths based on maui-blazor-web template structure
		var mauiAppProjectDir = Path.Combine(solutionProjectDir, solutionName);
		var mauiAppProjectFile = Path.Combine(mauiAppProjectDir, $"{solutionName}.csproj");
		var sharedRclDir = Path.Combine(solutionProjectDir, $"{solutionName}.Shared");

		_output.WriteLine($"Creating maui-blazor-web template in {solutionProjectDir}");

		// Create the template
		Assert.True(DotnetInternal.New(templateShortName, outputDirectory: solutionProjectDir, framework: DotNetCurrent, output: _output),
			$"Unable to create template {templateShortName}. Check test output for errors.");

		// Add a test file to the shared RCL's wwwroot to ensure we have RCL assets
		var testJsPath = Path.Combine(sharedRclDir, "wwwroot", "test-compression.js");
		File.WriteAllText(testJsPath, "// Test file for compression validation\nconsole.log('test');");
		_output.WriteLine($"Added test file: {testJsPath}");

		// Build the MAUI app for Android
		var buildProps = BuildProps;
		buildProps.Add("TrimMode=partial"); // Faster build

		_output.WriteLine($"Publishing MAUI app for Android: {mauiAppProjectFile}");
		Assert.True(DotnetInternal.Publish(mauiAppProjectFile, config, framework: $"{DotNetCurrent}-android", properties: buildProps, output: _output),
			$"Project {Path.GetFileName(mauiAppProjectFile)} failed to publish. Check test output/attachments for errors.");

		// Find the APK
		var apkSearchDir = Path.Combine(mauiAppProjectDir, "bin", config, $"{DotNetCurrent}-android", "publish");
		var apkFiles = Directory.GetFiles(apkSearchDir, "*-Signed.apk", SearchOption.AllDirectories);
		Assert.True(apkFiles.Length > 0, $"No signed APK found in {apkSearchDir}");

		var apkPath = apkFiles[0];
		_output.WriteLine($"Found APK: {apkPath}");

		// Extract and analyze APK contents
		var extractDir = Path.Combine(TestDirectory, "apk-contents");
		ZipFile.ExtractToDirectory(apkPath, extractDir);

		// Search for .gz and .br files in the APK
		var gzFiles = Directory.GetFiles(extractDir, "*.gz", SearchOption.AllDirectories);
		var brFiles = Directory.GetFiles(extractDir, "*.br", SearchOption.AllDirectories);

		_output.WriteLine($"Found {gzFiles.Length} .gz files and {brFiles.Length} .br files in APK");

		// Log any compressed files found (for debugging)
		foreach (var file in gzFiles.Concat(brFiles))
		{
			_output.WriteLine($"  Compressed file: {file.Replace(extractDir, "", StringComparison.Ordinal)}");
		}

		// Assert: No precompressed files should be in the MAUI APK
		// These files bloat the app bundle since Blazor Hybrid serves assets locally
		Assert.True(gzFiles.Length == 0, 
			$"APK should not contain .gz files but found {gzFiles.Length}. " +
			$"See https://github.com/dotnet/maui/issues/33773. Files: {string.Join(", ", gzFiles.Select(f => Path.GetFileName(f)))}");
		
		Assert.True(brFiles.Length == 0, 
			$"APK should not contain .br files but found {brFiles.Length}. " +
			$"See https://github.com/dotnet/maui/issues/33773. Files: {string.Join(", ", brFiles.Select(f => Path.GetFileName(f)))}");

		// Verify the original assets ARE present (not compressed versions)
		var wwwrootDir = Path.Combine(extractDir, "assets", "wwwroot");
		Assert.True(Directory.Exists(wwwrootDir), $"wwwroot directory should exist in APK at {wwwrootDir}");

		// Check that our test file exists (uncompressed)
		var sharedContentDir = Directory.GetDirectories(Path.Combine(wwwrootDir, "_content"), "*Shared*", SearchOption.TopDirectoryOnly);
		Assert.True(sharedContentDir.Length > 0, "Shared RCL content directory should exist in APK");
		
		var testJsInApk = Path.Combine(sharedContentDir[0], "test-compression.js");
		Assert.True(File.Exists(testJsInApk), $"Test JS file should exist uncompressed at {testJsInApk}");
		
		_output.WriteLine("✅ APK correctly contains no precompressed .gz/.br files from RCL");
	}

	/// <summary>
	/// Complementary test to MauiBlazorWebApk_DoesNotContainCompressedRclAssets.
	/// Verifies that the Blazor Web app from the same maui-blazor-web template
	/// DOES contain precompressed .gz/.br files, ensuring our MAUI fix doesn't
	/// break web compression which is needed for HTTP serving.
	/// </summary>
	[Fact]
	public void BlazorWebPublish_ContainsCompressedRclAssets()
	{
		SetTestIdentifier("BlazorWebPublish_HasCompression");
		const string templateShortName = "maui-blazor-web";
		const string config = "Release";

		var solutionProjectDir = TestDirectory;
		var solutionName = Path.GetFileName(solutionProjectDir);

		// Project paths based on maui-blazor-web template structure
		var webAppProjectDir = Path.Combine(solutionProjectDir, $"{solutionName}.Web");
		var webAppProjectFile = Path.Combine(webAppProjectDir, $"{solutionName}.Web.csproj");
		var sharedRclDir = Path.Combine(solutionProjectDir, $"{solutionName}.Shared");

		_output.WriteLine($"Creating maui-blazor-web template in {solutionProjectDir}");

		// Create the template
		Assert.True(DotnetInternal.New(templateShortName, outputDirectory: solutionProjectDir, framework: DotNetCurrent, output: _output),
			$"Unable to create template {templateShortName}. Check test output for errors.");

		// Add a test file to the shared RCL's wwwroot to ensure we have RCL assets
		var testJsPath = Path.Combine(sharedRclDir, "wwwroot", "test-compression.js");
		File.WriteAllText(testJsPath, "// Test file for compression validation\nconsole.log('test');");
		_output.WriteLine($"Added test file: {testJsPath}");

		// Publish the Web app
		_output.WriteLine($"Publishing Blazor Web app: {webAppProjectFile}");
		Assert.True(DotnetInternal.Publish(webAppProjectFile, config, properties: BuildProps, output: _output),
			$"Project {Path.GetFileName(webAppProjectFile)} failed to publish. Check test output/attachments for errors.");

		// Check the publish output for compressed files
		var publishDir = Path.Combine(webAppProjectDir, "bin", config, DotNetCurrent, "publish", "wwwroot");
		Assert.True(Directory.Exists(publishDir), $"Publish wwwroot directory should exist at {publishDir}");

		// Search for .gz and .br files in the published output
		var gzFiles = Directory.GetFiles(publishDir, "*.gz", SearchOption.AllDirectories);
		var brFiles = Directory.GetFiles(publishDir, "*.br", SearchOption.AllDirectories);

		_output.WriteLine($"Found {gzFiles.Length} .gz files and {brFiles.Length} .br files in web publish output");

		// Log some compressed files found (for debugging)
		foreach (var file in gzFiles.Take(5))
		{
			_output.WriteLine($"  .gz file: {file.Replace(publishDir, "", StringComparison.Ordinal)}");
		}
		foreach (var file in brFiles.Take(5))
		{
			_output.WriteLine($"  .br file: {file.Replace(publishDir, "", StringComparison.Ordinal)}");
		}

		// Assert: Web app SHOULD have precompressed files for HTTP serving
		Assert.True(gzFiles.Length > 0, 
			"Web app publish output should contain .gz files for HTTP compression. " +
			"If this fails, the compression system may be broken.");
		
		Assert.True(brFiles.Length > 0, 
			"Web app publish output should contain .br files for HTTP compression. " +
			"If this fails, the compression system may be broken.");

		// Verify our test file has compressed versions
		var sharedContentDir = Path.Combine(publishDir, "_content");
		var testJsGz = Directory.GetFiles(sharedContentDir, "test-compression.js.gz", SearchOption.AllDirectories);
		var testJsBr = Directory.GetFiles(sharedContentDir, "test-compression.js.br", SearchOption.AllDirectories);

		Assert.True(testJsGz.Length > 0, 
			"Test JS file should have a .gz compressed version in web publish output");
		Assert.True(testJsBr.Length > 0, 
			"Test JS file should have a .br compressed version in web publish output");

		_output.WriteLine("✅ Web app correctly contains precompressed .gz/.br files from RCL");
	}
}
