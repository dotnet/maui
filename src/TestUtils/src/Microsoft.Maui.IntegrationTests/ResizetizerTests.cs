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

	// Regression test for https://github.com/dotnet/maui/issues/23268: custom font assets were not
	// copied into the Android assets folder on the *first* (clean) Release build — they only showed
	// up after a second build. The fix makes _CollectMauiFontItems always run and map font paths
	// predictively from @(MauiFont) instead of relying on a filesystem glob that was empty during
	// first-build output inference. Release is required: the bug only reproduced in Release.
	[Theory]
	[InlineData("Release")]
	public void FontsAreCopiedToAndroidAssetsOnFirstBuild(string config)
	{
		SetTestIdentifier(config);

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		// The default maui template registers <MauiFont Include="Resources\Fonts\*" />, which includes
		// OpenSans-Regular.ttf, so building it exercises the font pipeline without extra assets.
		Assert.True(DotnetInternal.New("maui", projectDir, DotNetCurrent, output: _output),
			"Unable to create template maui. Check test output for errors.");

		var framework = $"{DotNetCurrent}-android";
		var androidObjDir = Path.Combine(projectDir, "obj", config, framework);
		const string fontFileName = "OpenSans-Regular.ttf";

		// First (clean) build for Android only.
		var firstBinlog = Path.Combine(projectDir, "first.binlog");
		Assert.True(DotnetInternal.Build(projectFile, config, framework: framework, properties: BuildProps, binlogPath: firstBinlog, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to build (first build). Check test output/attachments for errors.");

		Assert.True(FontExistsInAndroidAssets(androidObjDir, fontFileName),
			$"Font '{fontFileName}' was not copied into the Android assets folder under '{androidObjDir}' on the first build (regression #23268).");

		// Second (incremental) build.
		var secondBinlog = Path.Combine(projectDir, "second.binlog");
		Assert.True(DotnetInternal.Build(projectFile, config, framework: framework, properties: BuildProps, binlogPath: secondBinlog, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to build (incremental build). Check test output/attachments for errors.");

		// ProcessMauiFonts is incremental and should be skipped (up-to-date) on the second build,
		// while the always-run _CollectMauiFontItems must still execute and re-register the items.
		Assert.True(WasTargetSkipped(secondBinlog, "ProcessMauiFonts"),
			"ProcessMauiFonts should have been skipped (up-to-date) on the incremental build.");
		Assert.True(WasTargetExecuted(secondBinlog, "_CollectMauiFontItems"),
			"_CollectMauiFontItems should run on every build, even when ProcessMauiFonts is skipped.");
		Assert.True(FontExistsInAndroidAssets(androidObjDir, fontFileName),
			$"Font '{fontFileName}' is missing from the Android assets folder after an incremental build.");
	}

	static bool FontExistsInAndroidAssets(string androidObjDir, string fontFileName)
	{
		if (!Directory.Exists(androidObjDir))
			return false;

		// The intermediate copy lives under resizetizer\f\; only the file staged under an "assets"
		// folder proves the font was actually registered as an AndroidAsset and packaged.
		return Directory.EnumerateFiles(androidObjDir, fontFileName, SearchOption.AllDirectories)
			.Any(path => path.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
				.Any(segment => string.Equals(segment, "assets", StringComparison.OrdinalIgnoreCase)));
	}

	static bool WasTargetSkipped(string binlogPath, string targetName)
		=> GetTargetStatus(binlogPath, targetName).skipped;

	static bool WasTargetExecuted(string binlogPath, string targetName)
	{
		var (started, skipped) = GetTargetStatus(binlogPath, targetName);
		// An up-to-date incremental target emits BOTH a TargetStarted and a TargetSkipped event, so
		// "executed" means it started and was not skipped.
		return started && !skipped;
	}

	static (bool started, bool skipped) GetTargetStatus(string binlogPath, string targetName)
	{
		bool started = false;
		bool skipped = false;
		if (File.Exists(binlogPath))
		{
			foreach (var record in new BinLogReader().ReadRecords(binlogPath))
			{
				switch (record.Args)
				{
					case TargetStartedEventArgs s when string.Equals(s.TargetName, targetName, StringComparison.Ordinal):
						started = true;
						break;
					case TargetSkippedEventArgs sk when string.Equals(sk.TargetName, targetName, StringComparison.Ordinal):
						skipped = true;
						break;
				}
			}
		}
		return (started, skipped);
	}
}
