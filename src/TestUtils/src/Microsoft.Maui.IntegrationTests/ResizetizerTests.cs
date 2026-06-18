using System.IO.Compression;
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
	public void AndroidPackageRegeneratesFontsAndSplashWhenIntermediateOutputsAreMissing()
	{
		SetTestIdentifier("AndroidMissingResizetizerOutputs");

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");
		var framework = $"{DotNetCurrent}-android";
		const string config = "Debug";

		Assert.True(DotnetInternal.New("maui", projectDir, DotNetCurrent, output: _output),
			$"Unable to create template maui. Check test output for errors.");

		StripNonAndroidTfms(projectFile, framework);

		var buildProps = BuildProps;
		buildProps.Add("AndroidPackageFormat=apk");

		Assert.True(DotnetInternal.Build(projectFile, config, target: "SignAndroidPackage", framework: framework, properties: buildProps, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

		AssertAndroidPackageContainsFontsAndSplash(projectDir, config, framework);

		var intermediateOutputPath = Path.Combine(projectDir, "obj", config, framework);
		DeleteDirectory(Path.Combine(intermediateOutputPath, "resizetizer", "f"));
		DeleteDirectory(Path.Combine(intermediateOutputPath, "resizetizer", "sp"));
		DeleteDirectory(Path.Combine(intermediateOutputPath, "android"));
		DeleteDirectory(Path.Combine(intermediateOutputPath, "lp"));
		DeleteDirectory(Path.Combine(intermediateOutputPath, "stamp"));

		foreach (var package in Directory.GetFiles(projectDir, "*.apk", SearchOption.AllDirectories))
			File.Delete(package);

		Assert.True(DotnetInternal.Build(projectFile, config, target: "SignAndroidPackage", framework: framework, properties: buildProps, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to rebuild. Check test output/attachments for errors.");

		AssertAndroidPackageContainsFontsAndSplash(projectDir, config, framework);

		var noOpBinlogPath = Path.Combine(projectDir, "no-op.binlog");
		Assert.True(DotnetInternal.Build(projectFile, config, target: "SignAndroidPackage", framework: framework, properties: buildProps, binlogPath: noOpBinlogPath, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to no-op rebuild. Check test output/attachments for errors.");

		AssertTargetSkipped(noOpBinlogPath, "ProcessMauiFonts");
		AssertTargetSkipped(noOpBinlogPath, "ProcessMauiSplashScreens");
		AssertAndroidPackageContainsFontsAndSplash(projectDir, config, framework);
	}

	static void AssertAndroidPackageContainsFontsAndSplash(string projectDir, string config, string framework)
	{
		var apkSearchDir = Path.Combine(projectDir, "bin", config, framework);
		var apkPath = Directory.GetFiles(apkSearchDir, "*-Signed.apk", SearchOption.AllDirectories)
			.OrderByDescending(File.GetLastWriteTimeUtc)
			.FirstOrDefault();

		Assert.True(apkPath is not null, $"No signed APK found in {apkSearchDir}");

		using var apk = ZipFile.OpenRead(apkPath!);
		var entries = apk.Entries
			.Select(entry => entry.FullName)
			.ToHashSet(StringComparer.Ordinal);

		Assert.Contains("assets/OpenSans-Regular.ttf", entries);
		Assert.Contains("assets/OpenSans-Semibold.ttf", entries);
		Assert.True(
			entries.Any(entry => entry.StartsWith("res/drawable-", StringComparison.Ordinal) &&
				entry.EndsWith("/splash.png", StringComparison.Ordinal)),
			$"APK {apkPath} does not contain generated splash screen raster assets.");
	}

	static void DeleteDirectory(string path)
	{
		if (Directory.Exists(path))
			Directory.Delete(path, recursive: true);
	}

	static void StripNonAndroidTfms(string projectFile, string androidTfm)
	{
		var tempFile = Path.GetTempFileName();
		try
		{
			using (var reader = File.OpenText(projectFile))
			using (var writer = File.CreateText(tempFile))
			{
				string? line;
				while ((line = reader.ReadLine()) is not null)
				{
					if (line.Contains("<TargetFrameworks ", StringComparison.Ordinal) &&
						line.Contains(" Condition=", StringComparison.Ordinal))
					{
						continue;
					}

					if (line.Contains("<TargetFrameworks>", StringComparison.Ordinal))
					{
						var indentation = line[..line.IndexOf('<', StringComparison.Ordinal)];
						line = $"{indentation}<TargetFrameworks>{androidTfm}</TargetFrameworks>";
					}

					writer.WriteLine(line);
				}
			}

			File.Copy(tempFile, projectFile, overwrite: true);
		}
		finally
		{
			File.Delete(tempFile);
		}
	}

	static void AssertTargetSkipped(string binlogPath, string targetName)
	{
		var skipped = new BinLogReader()
			.ReadRecords(binlogPath)
			.Any(record => record.Args is BuildMessageEventArgs { Message: string message } &&
				message.Contains($"Skipping target \"{targetName}\"", StringComparison.Ordinal));

		Assert.True(skipped, $"Expected target '{targetName}' to be skipped. See binlog: {binlogPath}");
	}
}
