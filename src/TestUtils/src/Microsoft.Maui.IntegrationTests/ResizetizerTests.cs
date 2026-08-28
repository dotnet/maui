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
	[InlineData("maui", "classlib", true)] // base TFM
	[InlineData("maui", "mauilib", true)] // platform TFMs
	[InlineData("maui-blazor", "classlib", true)] // base TFM
	[InlineData("maui-blazor", "mauilib", true)] // platform TFMs
												 // windows packaged/msix
	[InlineData("maui", "classlib", false)] // base TFM
	[InlineData("maui", "mauilib", false)] // platform TFMs
	[InlineData("maui-blazor", "classlib", false)] // base TFM
	[InlineData("maui-blazor", "mauilib", false)] // platform TFMs
	public void CollectsAssets(string id, string libid, bool unpackaged)
	{
		SetTestIdentifier(id, libid, unpackaged);

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

		SetWindowsPackageType(appFile, unpackaged);

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

		// build and assert Android
		var androidFramework = $"{DotNetCurrent}-android";
		Assert.True(DotnetInternal.Build(appFile, "Debug", framework: androidFramework, properties: BuildProps, output: _output),
			$"Project {Path.GetFileName(appFile)} failed to build for Android. Check test output/attachments for errors.");
		Assert.True(File.Exists(Path.Combine(appDir, "obj", "Debug", androidFramework, "resizetizer", "r", "drawable-mdpi", "the_image.png")),
			"Android was missing the image file.");

		// build and assert iOS
		var iosFramework = $"{DotNetCurrent}-ios";
		var iosRuntimeIdentifier = TestEnvironment.IOSSimulatorRuntimeIdentifier;
		Assert.True(DotnetInternal.Build(appFile, "Debug", framework: iosFramework, properties: BuildProps, runtimeIdentifier: iosRuntimeIdentifier, output: _output),
			$"Project {Path.GetFileName(appFile)} failed to build for iOS. Check test output/attachments for errors.");
		Assert.True(File.Exists(Path.Combine(appDir, "obj", "Debug", iosFramework, iosRuntimeIdentifier, "resizetizer", "r", "the_image.png")),
			"iOS was missing the image file.");

		// build and assert Mac Catalyst
		var macCatalystFramework = $"{DotNetCurrent}-maccatalyst";
		var macCatalystRuntimeIdentifier = TestEnvironment.IsArm64 ? "maccatalyst-arm64" : "maccatalyst-x64";
		Assert.True(DotnetInternal.Build(appFile, "Debug", framework: macCatalystFramework, properties: BuildProps, runtimeIdentifier: macCatalystRuntimeIdentifier, output: _output),
			$"Project {Path.GetFileName(appFile)} failed to build for Mac Catalyst. Check test output/attachments for errors.");
		Assert.True(File.Exists(Path.Combine(appDir, "obj", "Debug", macCatalystFramework, macCatalystRuntimeIdentifier, "resizetizer", "r", "the_image.png")),
			"Mac Catalyst was missing the image file.");

		if (TestEnvironment.IsWindows)
		{
			var windowsFramework = $"{DotNetCurrent}-windows10.0.19041.0";
			Assert.True(DotnetInternal.Build(appFile, "Debug", framework: windowsFramework, properties: GetWindowsBuildProps(), output: _output),
				$"Project {Path.GetFileName(appFile)} failed to build for Windows. Check test output/attachments for errors.");
			Assert.True(File.Exists(Path.Combine(appDir, "obj", "x64", "Debug", windowsFramework, "resizetizer", "r", "the_image.scale-100.png")),
				"Windows was missing the image file.");
		}
	}

	[Theory]
	[InlineData("maui", "mauilib", true)]
	[InlineData("maui", "mauilib", false)]
	public void AdditionalPropertiesExcludesImage(string id, string libid, bool unpackaged)
	{
		SetTestIdentifier(id, libid, unpackaged);

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

		// add a project reference with AdditionalProperties to exclude library images
		FileUtilities.ReplaceInFile(appFile,
			"</Project>",
			"""
			<ItemGroup>
				<ProjectReference Include="..\thelib\thelib.csproj" AdditionalProperties="ExcludeLibraryImage=true" />
			</ItemGroup>
			</Project>
			""");

		SetWindowsPackageType(appFile, unpackaged);

		// add the svg file to the library
		File.WriteAllText(Path.Combine(libDir, "the_image.svg"), BlankSvgContents);

		// add the <MauiImage> that is conditionally excluded based on AdditionalProperties
		FileUtilities.ReplaceInFile(libFile,
			"</Project>",
			"""
			<PropertyGroup>
				<UseMaui>true</UseMaui>
				<SingleProject>true</SingleProject>
			</PropertyGroup>
			<ItemGroup>
				<MauiImage Include="the_image.svg" />
				<MauiImage Remove="the_image.svg" Condition="'$(ExcludeLibraryImage)' == 'true'" />
			</ItemGroup>
			</Project>
			""");

		// build and assert Android - the image should NOT be collected because AdditionalProperties excluded it
		var androidFramework = $"{DotNetCurrent}-android";
		Assert.True(DotnetInternal.Build(appFile, "Debug", framework: androidFramework, properties: BuildProps, output: _output),
			$"Project {Path.GetFileName(appFile)} failed to build for Android. Check test output/attachments for errors.");
		Assert.False(File.Exists(Path.Combine(appDir, "obj", "Debug", androidFramework, "resizetizer", "r", "drawable-mdpi", "the_image.png")),
			"Android should NOT have the image file (AdditionalProperties should have excluded it).");

		if (TestEnvironment.IsWindows)
		{
			var windowsFramework = $"{DotNetCurrent}-windows10.0.19041.0";
			Assert.True(DotnetInternal.Build(appFile, "Debug", framework: windowsFramework, properties: GetWindowsBuildProps(), output: _output),
				$"Project {Path.GetFileName(appFile)} failed to build for Windows. Check test output/attachments for errors.");
			Assert.False(File.Exists(Path.Combine(appDir, "obj", "x64", "Debug", windowsFramework, "resizetizer", "r", "the_image.scale-100.png")),
				"Windows should NOT have the image file (AdditionalProperties should have excluded it).");
		}
	}

	[Theory]
	[InlineData("maui", "mauilib", true)]
	[InlineData("maui", "mauilib", false)]
	public void AdditionalPropertiesSelectsImageInLibrary(string id, string libid, bool unpackaged)
	{
		SetTestIdentifier(id, libid, unpackaged);

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

		// add a project reference with AdditionalProperties to select the alternate image
		FileUtilities.ReplaceInFile(appFile,
			"</Project>",
			"""
			<ItemGroup>
				<ProjectReference Include="..\thelib\thelib.csproj" AdditionalProperties="UseAlternateImage=true" />
			</ItemGroup>
			</Project>
			""");

		SetWindowsPackageType(appFile, unpackaged);

		// add two svg files to the library — the property selects which one is included
		File.WriteAllText(Path.Combine(libDir, "default_image.svg"), BlankSvgContents);
		File.WriteAllText(Path.Combine(libDir, "alternate_image.svg"), BlankSvgContents);

		// add <MauiImage> that conditionally includes one file or the other based on the property
		FileUtilities.ReplaceInFile(libFile,
			"</Project>",
			"""
			<PropertyGroup>
				<UseMaui>true</UseMaui>
				<SingleProject>true</SingleProject>
			</PropertyGroup>
			<ItemGroup>
				<MauiImage Condition="'$(UseAlternateImage)' != 'true'" Include="default_image.svg" />
				<MauiImage Condition="'$(UseAlternateImage)' == 'true'" Include="alternate_image.svg" />
			</ItemGroup>
			</Project>
			""");

		// build and assert Android
		var androidFramework = $"{DotNetCurrent}-android";
		Assert.True(DotnetInternal.Build(appFile, "Debug", framework: androidFramework, properties: BuildProps, output: _output),
			$"Project {Path.GetFileName(appFile)} failed to build for Android. Check test output/attachments for errors.");
		Assert.True(File.Exists(Path.Combine(appDir, "obj", "Debug", androidFramework, "resizetizer", "r", "drawable-mdpi", "alternate_image.png")),
			"Android was missing alternate_image — AdditionalProperties was not propagated.");
		Assert.False(File.Exists(Path.Combine(appDir, "obj", "Debug", androidFramework, "resizetizer", "r", "drawable-mdpi", "default_image.png")),
			"Android should NOT have default_image — AdditionalProperties should have selected the alternate.");

		if (TestEnvironment.IsWindows)
		{
			var windowsFramework = $"{DotNetCurrent}-windows10.0.19041.0";
			Assert.True(DotnetInternal.Build(appFile, "Debug", framework: windowsFramework, properties: GetWindowsBuildProps(), output: _output),
				$"Project {Path.GetFileName(appFile)} failed to build for Windows. Check test output/attachments for errors.");
			Assert.True(File.Exists(Path.Combine(appDir, "obj", "x64", "Debug", windowsFramework, "resizetizer", "r", "alternate_image.scale-100.png")),
				"Windows was missing alternate_image — AdditionalProperties was not propagated.");
			Assert.False(File.Exists(Path.Combine(appDir, "obj", "x64", "Debug", windowsFramework, "resizetizer", "r", "default_image.scale-100.png")),
				"Windows should NOT have default_image — AdditionalProperties should have selected the alternate.");
		}
	}

	static void SetWindowsPackageType(string projectFile, bool unpackaged)
	{
		FileUtilities.ReplaceInFile(
			projectFile,
			"<WindowsPackageType>None</WindowsPackageType>",
			$"<WindowsPackageType>{(unpackaged ? "None" : "MSIX")}</WindowsPackageType>");
	}

	List<string> GetWindowsBuildProps()
	{
		var properties = BuildProps;
		// Constrain restore without producing RID-specific app output that mismatches project-reference PRI output.
		properties.Add("RuntimeIdentifiers=win-x64");
		properties.Add("Platform=x64");
		properties.Add("UseMonoRuntime=false");
		return properties;
	}
}
