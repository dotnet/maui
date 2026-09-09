namespace Microsoft.Maui.IntegrationTests;

sealed class WindowsOnlyTheoryAttribute : TheoryAttribute
{
	public WindowsOnlyTheoryAttribute()
	{
		if (!TestEnvironment.IsWindows)
			Skip = "Running Windows templates is only supported on Windows.";
	}
}

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

	static string AppleArchitecture => TestEnvironment.IsArm64 ? "arm64" : "x64";

	static string WindowsRuntimeIdentifier => TestEnvironment.IsArm64 ? "win-arm64" : "win-x64";

	[WindowsOnlyTheory]
	// windows unpackaged/exe
	[InlineData("maui", "classlib", true)]
	[InlineData("maui", "mauilib", true)]
	[InlineData("maui-blazor", "classlib", true)]
	[InlineData("maui-blazor", "mauilib", true)]
	// windows packaged/msix
	[InlineData("maui", "classlib", false)]
	[InlineData("maui", "mauilib", false)]
	[InlineData("maui-blazor", "classlib", false)]
	[InlineData("maui-blazor", "mauilib", false)]
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

		// toggle packaged / unpackaged
		if (!unpackaged)
		{
			FileUtilities.ReplaceInFile(appFile,
				"<WindowsPackageType>None</WindowsPackageType>",
				"");
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

		BuildResizetizerTargets(appFile);

		// assert
		Assert.True(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-android\\resizetizer\\r\\drawable-mdpi\\the_image.png")),
			"Android was missing the image file.");
		Assert.True(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-ios\\iossimulator-{AppleArchitecture}\\resizetizer\\r\\the_image.png")),
			"iOS was missing the image file.");
		Assert.True(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-maccatalyst\\maccatalyst-{AppleArchitecture}\\resizetizer\\r\\the_image.png")),
			"Mac Catalyst was missing the image file.");
		Assert.True(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-windows10.0.19041.0\\{WindowsRuntimeIdentifier}\\resizetizer\\r\\the_image.scale-100.png")),
			"Windows was missing the image file.");
	}

	[WindowsOnlyTheory]
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

		// toggle packaged / unpackaged
		if (!unpackaged)
		{
			FileUtilities.ReplaceInFile(appFile,
				"<WindowsPackageType>None</WindowsPackageType>",
				"");
		}

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

		BuildResizetizerTargets(appFile);

		// assert - the image should NOT be collected because AdditionalProperties excluded it
		Assert.False(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-android\\resizetizer\\r\\drawable-mdpi\\the_image.png")),
			"Android should NOT have the image file (AdditionalProperties should have excluded it).");
		Assert.False(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-windows10.0.19041.0\\{WindowsRuntimeIdentifier}\\resizetizer\\r\\the_image.scale-100.png")),
			"Windows should NOT have the image file (AdditionalProperties should have excluded it).");
	}

	[WindowsOnlyTheory]
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

		// toggle packaged / unpackaged
		if (!unpackaged)
		{
			FileUtilities.ReplaceInFile(appFile,
				"<WindowsPackageType>None</WindowsPackageType>",
				"");
		}

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

		BuildResizetizerTargets(appFile);

		// assert - alternate_image should be collected (property was propagated)
		Assert.True(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-android\\resizetizer\\r\\drawable-mdpi\\alternate_image.png")),
			"Android was missing alternate_image — AdditionalProperties was not propagated.");
		// assert - default_image should NOT be collected (it was excluded by the property)
		Assert.False(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-android\\resizetizer\\r\\drawable-mdpi\\default_image.png")),
			"Android should NOT have default_image — AdditionalProperties should have selected the alternate.");
		Assert.True(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-windows10.0.19041.0\\{WindowsRuntimeIdentifier}\\resizetizer\\r\\alternate_image.scale-100.png")),
			"Windows was missing alternate_image — AdditionalProperties was not propagated.");
		Assert.False(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-windows10.0.19041.0\\{WindowsRuntimeIdentifier}\\resizetizer\\r\\default_image.scale-100.png")),
			"Windows should NOT have default_image — AdditionalProperties should have selected the alternate.");
	}

	void BuildResizetizerTargets(string projectFile)
	{
		var targetFrameworks = new[]
		{
			$"{DotNetCurrent}-android",
			$"{DotNetCurrent}-ios",
			$"{DotNetCurrent}-maccatalyst",
			$"{DotNetCurrent}-windows10.0.19041.0",
		};

		foreach (var targetFramework in targetFrameworks)
		{
			Assert.True(
				DotnetInternal.Build(projectFile, "Debug", target: "ResizetizeImages", framework: targetFramework, properties: BuildProps, output: _output),
				$"Project {Path.GetFileName(projectFile)} failed to run Resizetizer for {targetFramework}. Check test output/attachments for errors.");
		}
	}
}
