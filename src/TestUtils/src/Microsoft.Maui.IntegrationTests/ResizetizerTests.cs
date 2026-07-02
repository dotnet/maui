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

	// Regression test for https://github.com/dotnet/maui/issues/33092 (consolidated from #35962):
	// after a successful build, deleting the generated font/splash intermediate outputs must
	// re-trigger ProcessMauiFonts / ProcessMauiSplashScreens on the next build. This is guaranteed by
	// tracking the generated files in mauifont.outputs / mauisplash.outputs manifests that feed the
	// targets' Outputs (see _ReadMauiFontOutputs / _ReadMauiSplashOutputs), replacing the old stamp
	// files whose timestamps could stay newer than the deleted outputs. A final no-op build asserts
	// both targets are skipped when nothing changed.
	[Fact]
	public void BuildRegeneratesFontsAndSplashWhenIntermediateOutputsAreMissing()
	{
		SetTestIdentifier("MissingResizetizerOutputs");

		// Builds every TFM of the default maui template (Android + iOS + MacCatalyst), which only
		// fully builds on macOS, so this is gated accordingly. The Android first-build path is
		// additionally covered on Windows by FontsAreCopiedToAndroidAssetsOnFirstBuild.
		if (!TestEnvironment.IsMacOS)
			return;

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

	static bool FontExistsInAndroidAssets(string androidObjDir, string fontFileName)
	{
		// A registered AndroidAsset is staged by .NET for Android into $(IntermediateOutputPath)assets/,
		// i.e. obj/{config}/{tfm}/assets/ — a deterministic path (no RID segment for a default build).
		// The intermediate resizetizer copy under resizetizer/f/ is NOT proof of registration; only the
		// staged copy under assets/ is. Confirmed empirically with a Release net*-android build.
		return File.Exists(Path.Combine(androidObjDir, "assets", fontFileName));
	}

	static bool WasTargetSkipped(string binlogPath, string targetName)
	{
		var (started, skipped) = GetTargetStatus(binlogPath, targetName);
		// A skipped (up-to-date) invocation emits BOTH a TargetStarted and a TargetSkipped event,
		// while an executed invocation emits only TargetStarted. In multi-RID / outer+inner builds a
		// target can be invoked several times, so the target counts as "skipped" only when every
		// invocation was skipped: at least one skip and no net executions (started == skipped).
		return skipped > 0 && started == skipped;
	}

	static bool WasTargetExecuted(string binlogPath, string targetName)
	{
		var (started, skipped) = GetTargetStatus(binlogPath, targetName);
		// Each skipped invocation consumes one TargetStarted, so the number of real executions is
		// (started - skipped). The target executed if at least one invocation actually ran.
		return started > skipped;
	}

	static (int started, int skipped) GetTargetStatus(string binlogPath, string targetName)
	{
		int started = 0;
		int skipped = 0;
		if (File.Exists(binlogPath))
		{
			foreach (var record in new BinLogReader().ReadRecords(binlogPath))
			{
				switch (record.Args)
				{
					case TargetStartedEventArgs s when string.Equals(s.TargetName, targetName, StringComparison.Ordinal):
						started++;
						break;
					case TargetSkippedEventArgs sk when string.Equals(sk.TargetName, targetName, StringComparison.Ordinal):
						skipped++;
						break;
				}
			}
		}
		return (started, skipped);
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
		// This is only reached from BuildRegeneratesFontsAndSplashWhenIntermediateOutputsAreMissing,
		// which is gated to macOS (it builds the Apple TFMs), so only the Apple + Android roots are
		// asserted here. Windows is covered separately by FontsAreCopiedToAndroidAssetsOnFirstBuild's
		// sibling Windows lanes and CollectsAssets, and is never built by this macOS-only test.
		Assert.Contains(intermediateOutputRoots, root => ContainsTargetFramework(root, $"{DotNetCurrent}-android"));
		Assert.Contains(intermediateOutputRoots, root => ContainsTargetFramework(root, $"{DotNetCurrent}-ios"));
		Assert.Contains(intermediateOutputRoots, root => ContainsTargetFramework(root, $"{DotNetCurrent}-maccatalyst"));
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
				// Assert a deterministic generated splash marker per platform instead of scanning the
				// whole directory (confirmed with Release builds):
				//  - Android: resizetizer/sp/drawable/maui_splash_image.xml
				//  - iOS:     resizetizer/sp/MauiSplash.storyboard
				var splashMarker = ContainsTargetFramework(intermediateOutputRoot, $"{DotNetCurrent}-android")
					? Path.Combine(splashDir, "drawable", "maui_splash_image.xml")
					: Path.Combine(splashDir, "MauiSplash.storyboard");
				Assert.True(File.Exists(splashMarker),
					$"Missing generated splash marker '{splashMarker}'.");
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
		Assert.True(File.Exists(binlogPath), $"Binlog not found: {binlogPath}");

		// Count TargetSkippedEventArgs directly instead of matching localized/implementation-specific
		// log message text ("Skipping target ... because all output files are up-to-date"), which is
		// brittle across MSBuild versions and non-English locales.
		var (_, skipped) = GetTargetStatus(binlogPath, targetName);

		Assert.True(skipped >= minimumSkipCount,
			$"Expected target '{targetName}' to be skipped at least {minimumSkipCount} times, but found {skipped}. See binlog: {binlogPath}");
	}
}
