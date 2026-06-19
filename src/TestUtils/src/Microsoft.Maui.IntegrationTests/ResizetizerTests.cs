using Microsoft.Build.Framework;
using Microsoft.Build.Logging.StructuredLogger;

namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class ResizetizerTests : BaseBuildTest
{
	public ResizetizerTests(IntegrationTestFixture fixture, ITestOutputHelper output) : base(fixture, output) { }

	const string BlankSvgContents =
		"""
		<?xml version="1.0" encoding="UTF-8" standalone="no"?>
		<svg width="456" height="456" viewBox="0 0 456 456" version="1.1" xmlns="http://www.w3.org/2000/svg">
			<rect x="0" y="0" width="456" height="456" fill="#512BD4" />
		</svg>
		""";

	[Theory]
	// windows unpackaged/exe
	[InlineData("maui", "classlib", true)] // net9.0
	[InlineData("maui", "mauilib", true)] // net9.0-xxx
	[InlineData("maui-blazor", "classlib", true)] // net9.0
	[InlineData("maui-blazor", "mauilib", true)] // net9.0-xxx
											   // windows packaged/msix
	[InlineData("maui", "classlib", false)] // net9.0
	[InlineData("maui", "mauilib", false)] // net9.0-xxx
	[InlineData("maui-blazor", "classlib", false)] // net9.0
	[InlineData("maui-blazor", "mauilib", false)] // net9.0-xxx
	public void CollectsAssets(string id, string libid, bool unpackaged)
	{
		SetTestIdentifier(id, libid, unpackaged);
		// TODO: fix the tests as they have been disabled too long!
		if (!TestEnvironment.IsWindows)
			if (true) return; // Skip: "Running Windows templates is only supported on Windows."

		// new app
		var appDir = Path.Combine(TestDirectory, "theapp");
		var appFile = Path.Combine(appDir, $"{Path.GetFileName(appDir)}.csproj");
		Assert.True(DotnetInternal.New(id, appDir, DotNetCurrent, output: _output),
			$"Unable to create template {id}. Check test output for errors.");

		// new lib
		var libDir = Path.Combine(TestDirectory, "thelib");
		var libFile = Path.Combine(libDir, $"{Path.GetFileName(libDir)}.csproj");
		Assert.True(DotnetInternal.New(libid, libDir, DotNetCurrent, output: _output),
			$"Unable to create template {libid}. Check test output for errors.");

		// add a project reference
		FileUtilities.ReplaceInFile(appFile,
			"</Project>",
			"""
			<ItemGroup>
				<ProjectReference Include="..\thelib\thelib.csproj" />
			</ItemGroup>
			</Project>
			""");

		// toggle packaged / unpackaged
		if (unpackaged)
		{
			FileUtilities.ReplaceInFile(appFile,
				"</Project>",
				"""
				<PropertyGroup>
					<WindowsPackageType>None</WindowsPackageType>
				</PropertyGroup>
				</Project>
				""");

		}

		// add the svg file
		File.WriteAllText(Path.Combine(libDir, "the_image.svg"), BlankSvgContents);

		// add the <MauiImage>
		FileUtilities.ReplaceInFile(libFile,
			"</Project>",
			"""
			<PropertyGroup>
				<UseMaui>true</UseMaui>
				<SingleProject>true</SingleProject>
			</PropertyGroup>
			<ItemGroup>
				<MauiImage Include="the_image.svg" />
			</ItemGroup>
			</Project>
			""");

		// build
		Assert.True(DotnetInternal.Build(appFile, "Debug", properties: BuildProps, output: _output),
			$"Project {Path.GetFileName(appFile)} failed to build. Check test output/attachments for errors.");

		// assert
		Assert.True(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-android\\resizetizer\\r\\drawable-mdpi\\the_image.png")),
			"Android was missing the image file.");
		Assert.True(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-ios\\iossimulator-x64\\resizetizer\\r\\the_image.png")),
			"iOS was missing the image file.");
		Assert.True(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-maccatalyst\\maccatalyst-x64\\resizetizer\\r\\the_image.png")),
			"Mac Catalyst was missing the image file.");
		if (TestEnvironment.IsWindows)
			Assert.True(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-windows10.0.19041.0\\win-x64\\resizetizer\\r\\the_image.scale-100.png")),
				"Windows was missing the image file.");
	}

	[Fact]
	public void BuildRegeneratesFontsAndSplashWhenIntermediateOutputsAreMissing()
	{
		SetTestIdentifier("MissingResizetizerOutputs");

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");
		const string config = "Debug";

		Assert.True(DotnetInternal.New("maui", projectDir, DotNetCurrent, output: _output),
			$"Unable to create template maui. Check test output for errors.");

		Assert.True(DotnetInternal.Build(projectFile, config, properties: BuildProps, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

		var intermediateOutputRoots = GetResizetizerOutputRoots(projectDir, config);
		AssertBuiltTargetPlatforms(intermediateOutputRoots);
		AssertIntermediateOutputsExist(intermediateOutputRoots);

		foreach (var intermediateOutputRoot in intermediateOutputRoots)
		{
			DeleteDirectory(Path.Combine(intermediateOutputRoot, "resizetizer", "f"));
			DeleteDirectory(Path.Combine(intermediateOutputRoot, "resizetizer", "sp"));
		}

		Assert.True(DotnetInternal.Build(projectFile, config, properties: BuildProps, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to rebuild. Check test output/attachments for errors.");

		AssertIntermediateOutputsExist(intermediateOutputRoots);

		var noOpBinlogPath = Path.Combine(projectDir, "no-op.binlog");
		Assert.True(DotnetInternal.Build(projectFile, config, properties: BuildProps, binlogPath: noOpBinlogPath, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to no-op rebuild. Check test output/attachments for errors.");

		AssertTargetSkipped(noOpBinlogPath, "ProcessMauiFonts", intermediateOutputRoots.Count);
		AssertTargetSkipped(noOpBinlogPath, "ProcessMauiSplashScreens", intermediateOutputRoots.Count);
		AssertIntermediateOutputsExist(intermediateOutputRoots);
	}

	static IReadOnlyList<string> GetResizetizerOutputRoots(string projectDir, string config)
	{
		var intermediateOutputPath = Path.Combine(projectDir, "obj", config);
		var outputRoots = Directory
			.GetFiles(intermediateOutputPath, "mauifont.outputs", SearchOption.AllDirectories)
			.Select(Path.GetDirectoryName)
			.Where(root => root is not null && File.Exists(Path.Combine(root, "mauisplash.outputs")))
			.Cast<string>()
			.OrderBy(root => root, StringComparer.OrdinalIgnoreCase)
			.ToArray();

		Assert.NotEmpty(outputRoots);
		return outputRoots;
	}

	static void AssertBuiltTargetPlatforms(IReadOnlyList<string> intermediateOutputRoots)
	{
		Assert.Contains(intermediateOutputRoots, root => ContainsTargetFramework(root, $"{DotNetCurrent}-android"));
		Assert.Contains(intermediateOutputRoots, root => ContainsTargetFramework(root, $"{DotNetCurrent}-ios"));
		Assert.Contains(intermediateOutputRoots, root => ContainsTargetFramework(root, $"{DotNetCurrent}-maccatalyst"));

		if (TestEnvironment.IsWindows)
			Assert.Contains(intermediateOutputRoots, root => ContainsTargetFramework(root, $"{DotNetCurrent}-windows"));
	}

	static bool ContainsTargetFramework(string path, string targetFramework) =>
		path.Contains(targetFramework, StringComparison.OrdinalIgnoreCase);

	static void AssertIntermediateOutputsExist(IReadOnlyList<string> intermediateOutputRoots)
	{
		foreach (var intermediateOutputRoot in intermediateOutputRoots)
		{
			var fontsDir = Path.Combine(intermediateOutputRoot, "resizetizer", "f");
			Assert.True(File.Exists(Path.Combine(fontsDir, "OpenSans-Regular.ttf")),
				$"Missing OpenSans-Regular.ttf in {fontsDir}.");
			Assert.True(File.Exists(Path.Combine(fontsDir, "OpenSans-Semibold.ttf")),
				$"Missing OpenSans-Semibold.ttf in {fontsDir}.");

			if (!ContainsTargetFramework(intermediateOutputRoot, $"{DotNetCurrent}-maccatalyst"))
			{
				var splashDir = Path.Combine(intermediateOutputRoot, "resizetizer", "sp");
				Assert.True(
					Directory.Exists(splashDir) && Directory.EnumerateFiles(splashDir, "*", SearchOption.AllDirectories).Any(),
					$"Missing generated splash screen files in {splashDir}.");
			}
		}
	}

	static void DeleteDirectory(string path)
	{
		if (Directory.Exists(path))
			Directory.Delete(path, recursive: true);
	}

	static void AssertTargetSkipped(string binlogPath, string targetName, int minimumSkipCount)
	{
		var skipCount = new BinLogReader()
			.ReadRecords(binlogPath)
			.Count(record => record.Args is BuildMessageEventArgs { Message: string message } &&
				message.Contains($"Skipping target \"{targetName}\"", StringComparison.Ordinal) &&
				message.Contains("because all output files are up-to-date", StringComparison.OrdinalIgnoreCase));

		Assert.True(skipCount >= minimumSkipCount,
			$"Expected target '{targetName}' to be skipped at least {minimumSkipCount} times, but found {skipCount}. See binlog: {binlogPath}");
	}
}
