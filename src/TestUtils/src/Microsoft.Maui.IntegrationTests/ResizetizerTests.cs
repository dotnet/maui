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

	[Theory]
	[InlineData("maui", "mauilib", true)]
	[InlineData("maui", "mauilib", false)]
	public void AdditionalPropertiesExcludesImage(string id, string libid, bool unpackaged)
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

		// build
		Assert.True(DotnetInternal.Build(appFile, "Debug", properties: BuildProps, output: _output),
			$"Project {Path.GetFileName(appFile)} failed to build. Check test output/attachments for errors.");

		// assert - the image should NOT be collected because AdditionalProperties excluded it
		Assert.False(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-android\\resizetizer\\r\\drawable-mdpi\\the_image.png")),
			"Android should NOT have the image file (AdditionalProperties should have excluded it).");
		if (TestEnvironment.IsWindows)
			Assert.False(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-windows10.0.19041.0\\win-x64\\resizetizer\\r\\the_image.scale-100.png")),
				"Windows should NOT have the image file (AdditionalProperties should have excluded it).");
	}

	[Theory]
	[InlineData("maui", "mauilib", true)]
	[InlineData("maui", "mauilib", false)]
	public void AdditionalPropertiesPassesColorToLibrary(string id, string libid, bool unpackaged)
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

		// add a project reference with AdditionalProperties to override the image color
		FileUtilities.ReplaceInFile(appFile,
			"</Project>",
			"""
			<ItemGroup>
				<ProjectReference Include="..\thelib\thelib.csproj" AdditionalProperties="LibraryImageColor=%23FF0000" />
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

		// add the svg file to the library (white fill, so color tint is visible)
		File.WriteAllText(Path.Combine(libDir, "the_image.svg"), BlankSvgContents);

		// add the <MauiImage> with Color controlled by a property
		FileUtilities.ReplaceInFile(libFile,
			"</Project>",
			"""
			<PropertyGroup>
				<UseMaui>true</UseMaui>
				<SingleProject>true</SingleProject>
				<LibraryImageColor Condition="'$(LibraryImageColor)' == ''">#0000FF</LibraryImageColor>
			</PropertyGroup>
			<ItemGroup>
				<MauiImage Include="the_image.svg" Color="$(LibraryImageColor)" />
			</ItemGroup>
			</Project>
			""");

		// build
		Assert.True(DotnetInternal.Build(appFile, "Debug", properties: BuildProps, output: _output),
			$"Project {Path.GetFileName(appFile)} failed to build. Check test output/attachments for errors.");

		// assert - image should exist (it's not excluded, just colored differently)
		Assert.True(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-android\\resizetizer\\r\\drawable-mdpi\\the_image.png")),
			"Android was missing the image file.");
		if (TestEnvironment.IsWindows)
			Assert.True(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-windows10.0.19041.0\\win-x64\\resizetizer\\r\\the_image.scale-100.png")),
				"Windows was missing the image file.");
	}
}
