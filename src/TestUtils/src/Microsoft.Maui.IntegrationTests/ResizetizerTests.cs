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
	public void CustomBackendProcessesImagesWithoutBuiltInOutputInjection()
	{
		SetTestIdentifier("custom-backend");
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, "CustomBackend.csproj");
		var imageFile = Path.Combine(projectDir, "image.svg");

		File.WriteAllText(imageFile, BlankSvgContents);
		File.WriteAllText(projectFile,
			$$"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>{{DotNetCurrent}}</TargetFramework>
			    <ResizetizerPlatformType>custom-backend</ResizetizerPlatformType>
			    <ResizetizerAfterImageProcessingTargets>VerifyCustomBackendImages</ResizetizerAfterImageProcessingTargets>
			  </PropertyGroup>
			  <ItemGroup>
			    <PackageReference Include="Microsoft.Maui.Resizetizer" Version="{{MauiPackageVersion}}" />
			    <MauiImage Include="image.svg" />
			  </ItemGroup>
			  <Target Name="VerifyCustomBackendImages">
			    <Error Condition="'@(MauiImage)' != '' And '@(MauiProcessedImage)' == ''" Text="Custom backends must receive processed images." />
			    <Error Condition="'@(ContentWithTargetPath)' != ''" Text="Custom backends must not receive built-in output injection." />
			    <WriteLinesToFile File="$(_MauiIntermediateImages)custom-backend.items" Lines="@(MauiProcessedImage)" Overwrite="true" />
			  </Target>
			  <Target Name="VerifyCustomBackendResources" DependsOnTargets="ResizetizeImages" />
			</Project>
			""");

		Assert.True(DotnetInternal.Build(projectFile, "Debug", target: "VerifyCustomBackendResources", properties: BuildProps, output: _output),
			$"Custom backend project failed to process images. Check test output for errors.");

		var outputsFile = Path.Combine(projectDir, "obj", "Debug", DotNetCurrent, "resizetizer", "r", "custom-backend.items");
		Assert.True(File.Exists(outputsFile), $"Custom backend output list was not created: {outputsFile}");
		var processedImages = File.ReadAllLines(outputsFile);
		Assert.Equal(2, processedImages.Length);
		Assert.All(processedImages, path => Assert.True(File.Exists(path), $"Processed image does not exist: {path}"));

		// A genuine no-op rebuild must skip ResizetizeImages and restore only the
		// persisted Resizetizer output list, not backend-written files from the folder.
		const string incrementalBinlog = "custom-backend-incremental.binlog";
		Assert.True(DotnetInternal.Build(projectFile, "Debug", target: "VerifyCustomBackendResources", properties: BuildProps, binlogPath: incrementalBinlog, output: _output),
			"Custom backend project failed on incremental rebuild. Check test output for errors.");
		Assert.True(
			WasTargetSkippedAsUpToDate(Path.Combine(projectDir, incrementalBinlog), "ResizetizeImages"),
			"ResizetizeImages should be skipped as up-to-date on the no-op rebuild.");

		var processedImagesIncremental = File.ReadAllLines(outputsFile);
		Assert.Equal(2, processedImagesIncremental.Length);
		Assert.DoesNotContain(processedImagesIncremental, path => path.EndsWith(".items", StringComparison.OrdinalIgnoreCase));

		// Removing the final image must not restore the previous output list. The old
		// generated files are deleted and no processed images flow to the backend.
		File.WriteAllText(
			projectFile,
			File.ReadAllText(projectFile).Replace(
				"""    <MauiImage Include="image.svg" />""",
				string.Empty,
				StringComparison.Ordinal));

		Assert.True(DotnetInternal.Build(projectFile, "Debug", target: "VerifyCustomBackendResources", properties: BuildProps, output: _output),
			"Custom backend project failed after removing its final image. Check test output for errors.");

		Assert.Empty(File.ReadAllLines(outputsFile));
		Assert.All(processedImages, path => Assert.False(File.Exists(path), $"Stale processed image still exists: {path}"));
	}

	[Fact]
	public void CustomBackendLateImportCollectsReferencedResourcesWithoutBuiltInOutputInjection()
	{
		SetTestIdentifier("custom-backend-late-import");
		var projectDir = TestDirectory;

		// A referenced library provides the MauiImage/MauiFont/MauiAsset items. These are only
		// surfaced to the app through the ResizetizeCollectItems prerequisite, so the custom
		// backend gets nothing unless the collection/input-output wiring runs.
		var libDir = Path.Combine(projectDir, "ResLib");
		Directory.CreateDirectory(libDir);
		File.WriteAllText(Path.Combine(libDir, "lib_image.svg"), BlankSvgContents);
		File.WriteAllText(Path.Combine(libDir, "lib_font.ttf"), "not-a-real-font");
		File.WriteAllText(Path.Combine(libDir, "lib_asset.txt"), "asset-contents");
		// The app and referenced library intentionally contribute the same font filename.
		// The physical copy output and MauiProcessedFont contract must contain it only once.
		File.WriteAllText(Path.Combine(projectDir, "lib_font.ttf"), "not-a-real-font");
		File.WriteAllText(Path.Combine(libDir, "ResLib.csproj"),
			$$"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>{{DotNetCurrent}}</TargetFramework>
			  </PropertyGroup>
			  <ItemGroup>
			    <PackageReference Include="Microsoft.Maui.Resizetizer" Version="{{MauiPackageVersion}}" />
			    <MauiImage Include="lib_image.svg" />
			    <MauiFont Include="lib_font.ttf" />
			    <MauiAsset Include="lib_asset.txt" LogicalName="lib_asset.txt" />
			  </ItemGroup>
			</Project>
			""");

		// The backend opts in by setting ResizetizerPlatformType from a targets file that is
		// imported *after* the Resizetizer targets. At that point the evaluation-time
		// dependency properties were already skipped, so processing relies on the
		// execution-time _PrepareExternalMaui* fallback targets to run the prerequisites.
		File.WriteAllText(Path.Combine(projectDir, "custom-backend.targets"),
			"""
			<Project>
			  <PropertyGroup>
			    <ResizetizerPlatformType>custom-backend</ResizetizerPlatformType>
			    <ResizetizerAfterImageProcessingTargets>VerifyCustomBackendImages</ResizetizerAfterImageProcessingTargets>
			    <ResizetizerAfterFontProcessingTargets>VerifyCustomBackendFonts</ResizetizerAfterFontProcessingTargets>
			    <ResizetizerAfterAssetProcessingTargets>VerifyCustomBackendAssets</ResizetizerAfterAssetProcessingTargets>
			  </PropertyGroup>
			  <Target Name="VerifyCustomBackendImages">
			    <Error Condition="'@(MauiProcessedImage)' == ''" Text="Custom backends must receive processed images collected from references." />
			    <Error Condition="'@(ContentWithTargetPath)' != ''" Text="Custom backends must not receive built-in output injection." />
			    <WriteLinesToFile File="$(_MauiIntermediateImages)late-import.images" Lines="@(MauiProcessedImage)" Overwrite="true" />
			  </Target>
			  <Target Name="VerifyCustomBackendFonts">
			    <Error Condition="'$(ExpectNoProcessedFonts)' != 'true' And '@(MauiProcessedFont)' == ''" Text="Custom backends must receive processed fonts collected from references." />
			    <Error Condition="'$(ExpectNoProcessedFonts)' == 'true' And '@(MauiProcessedFont)' != ''" Text="Removed fonts must not remain in the processed font contract." />
			    <WriteLinesToFile File="$(_MauiIntermediateImages)late-import.fonts" Lines="@(MauiProcessedFont)" Overwrite="true" />
			  </Target>
			  <Target Name="VerifyCustomBackendAssets">
			    <Error Condition="'@(MauiProcessedAsset)' == ''" Text="Custom backends must receive processed assets collected from references." />
			    <WriteLinesToFile File="$(_MauiIntermediateImages)late-import.assets" Lines="@(MauiProcessedAsset)" Overwrite="true" />
			  </Target>
			  <Target Name="VerifyCustomBackendResources" DependsOnTargets="ResizetizeImages;ProcessMauiFonts;ProcessMauiAssets" />
			</Project>
			""");

		// Use explicit SDK imports so the backend targets can be imported *after* the
		// Resizetizer targets (a plain trailing <Import> still evaluates before the SDK's
		// implicit package imports, which would opt in at evaluation time instead).
		var projectFile = Path.Combine(projectDir, "CustomBackend.csproj");
		File.WriteAllText(projectFile,
			$$"""
			<Project>
			  <Import Project="Sdk.props" Sdk="Microsoft.NET.Sdk" />
			  <PropertyGroup>
			    <TargetFramework>{{DotNetCurrent}}</TargetFramework>
			    <EnableDefaultItems>false</EnableDefaultItems>
			  </PropertyGroup>
			  <ItemGroup>
			    <PackageReference Include="Microsoft.Maui.Resizetizer" Version="{{MauiPackageVersion}}" />
			    <ProjectReference Include="ResLib\ResLib.csproj" />
			    <MauiFont Include="lib_font.ttf" />
			  </ItemGroup>
			  <Import Project="Sdk.targets" Sdk="Microsoft.NET.Sdk" />
			  <!-- Late import: this runs after the Resizetizer targets from the package. -->
			  <Import Project="custom-backend.targets" />
			</Project>
			""");

		Assert.True(DotnetInternal.Build(projectFile, "Debug", target: "VerifyCustomBackendResources", properties: BuildProps, output: _output),
			$"Custom backend project failed to process late-imported resources. Check test output for errors.");

		var intermediateDir = Path.Combine(projectDir, "obj", "Debug", DotNetCurrent, "resizetizer", "r");

		var imagesFile = Path.Combine(intermediateDir, "late-import.images");
		Assert.True(File.Exists(imagesFile), $"Custom backend image output list was not created: {imagesFile}");
		var processedImages = File.ReadAllLines(imagesFile);
		Assert.NotEmpty(processedImages);
		Assert.All(processedImages, path => Assert.True(File.Exists(path), $"Processed image does not exist: {path}"));

		var fontsFile = Path.Combine(intermediateDir, "late-import.fonts");
		Assert.True(File.Exists(fontsFile),
			"Custom backend font output list was not created; referenced fonts were not collected.");
		var processedFont = Assert.Single(File.ReadAllLines(fontsFile));
		var processedFontPath = Path.IsPathRooted(processedFont)
			? processedFont
			: Path.Combine(projectDir, processedFont);
		Assert.True(File.Exists(processedFontPath), $"Processed font does not exist: {processedFont}");

		var fontStampFile = Path.Combine(projectDir, "obj", "Debug", DotNetCurrent, "mauifont.stamp");
		Assert.True(File.Exists(fontStampFile), $"Font stamp file does not exist: {fontStampFile}");
		var fontStampWriteTime = File.GetLastWriteTimeUtc(fontStampFile);
		File.Delete(fontsFile);
		System.Threading.Thread.Sleep(TimeSpan.FromSeconds(1));
		Assert.True(DotnetInternal.Build(projectFile, "Debug", target: "VerifyCustomBackendResources", properties: BuildProps, output: _output),
			"Custom backend project failed on a no-op font rebuild. Check test output for errors.");
		Assert.Equal(fontStampWriteTime, File.GetLastWriteTimeUtc(fontStampFile));
		Assert.True(File.Exists(fontsFile), "Custom backend font output list was not recreated on the no-op rebuild.");
		Assert.Single(File.ReadAllLines(fontsFile));

		File.WriteAllText(
			projectFile,
			File.ReadAllText(projectFile).Replace(
				"""    <MauiFont Include="lib_font.ttf" />""",
				string.Empty,
				StringComparison.Ordinal));
		var libProjectFile = Path.Combine(libDir, "ResLib.csproj");
		File.WriteAllText(
			libProjectFile,
			File.ReadAllText(libProjectFile).Replace(
				"""    <MauiFont Include="lib_font.ttf" />""",
				string.Empty,
				StringComparison.Ordinal));

		var noFontBuildProps = BuildProps;
		noFontBuildProps.Add("ExpectNoProcessedFonts=true");
		Assert.True(DotnetInternal.Build(projectFile, "Debug", target: "VerifyCustomBackendResources", properties: noFontBuildProps, output: _output),
			"Custom backend project failed after removing its final font. Check test output for errors.");
		Assert.Empty(File.ReadAllLines(fontsFile));

		Assert.True(File.Exists(Path.Combine(intermediateDir, "late-import.assets")),
			"Custom backend asset output list was not created; referenced assets were not collected.");
	}

	[Fact]
	public void CustomBackendEarlyOptInCollectsReferencedAssets()
	{
		SetTestIdentifier("custom-backend-early-optin");
		var projectDir = TestDirectory;

		// A referenced library provides MauiAsset items.  The items are surfaced to the app
		// project only when ResizetizeCollectItems runs before ProcessMauiAssets.  For early
		// opt-in backends (ResizetizerPlatformType set at evaluation time) the execution-time
		// _PrepareExternalMauiAssets fallback is skipped because _ResizetizerIsCompatibleApp
		// is already True; instead ProcessMauiAssetsDependsOnTargets must include
		// ResizetizeCollectItems so that ProcessMauiAssets picks up the referenced items.
		var libDir = Path.Combine(projectDir, "ResLib");
		Directory.CreateDirectory(libDir);
		File.WriteAllText(Path.Combine(libDir, "lib_asset.txt"), "asset-contents");
		File.WriteAllText(Path.Combine(libDir, "ResLib.csproj"),
			$$"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>{{DotNetCurrent}}</TargetFramework>
			  </PropertyGroup>
			  <ItemGroup>
			    <PackageReference Include="Microsoft.Maui.Resizetizer" Version="{{MauiPackageVersion}}" />
			    <MauiAsset Include="lib_asset.txt" LogicalName="lib_asset.txt" />
			  </ItemGroup>
			</Project>
			""");

		// Early opt-in: ResizetizerPlatformType is set directly in the project body so
		// _ResizetizerIsCompatibleApp becomes True at evaluation time.
		var projectFile = Path.Combine(projectDir, "CustomBackend.csproj");
		File.WriteAllText(projectFile,
			$$"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>{{DotNetCurrent}}</TargetFramework>
			    <EnableDefaultItems>false</EnableDefaultItems>
			    <ResizetizerPlatformType>custom-backend</ResizetizerPlatformType>
			    <ResizetizerAfterAssetProcessingTargets>VerifyCustomBackendAssets</ResizetizerAfterAssetProcessingTargets>
			  </PropertyGroup>
			  <ItemGroup>
			    <PackageReference Include="Microsoft.Maui.Resizetizer" Version="{{MauiPackageVersion}}" />
			    <ProjectReference Include="ResLib\ResLib.csproj" />
			  </ItemGroup>
			  <Target Name="VerifyCustomBackendAssets">
			    <Error Condition="'@(MauiProcessedAsset)' == ''" Text="Early-opt-in backends must receive processed assets collected from project references." />
			    <WriteLinesToFile File="$(_MauiIntermediateImages)early-optin.assets" Lines="@(MauiProcessedAsset)" Overwrite="true" />
			  </Target>
			  <Target Name="VerifyCustomBackendResources" DependsOnTargets="ProcessMauiAssets" />
			</Project>
			""");

		Assert.True(DotnetInternal.Build(projectFile, "Debug", target: "VerifyCustomBackendResources", properties: BuildProps, output: _output),
			"Early opt-in custom backend project failed to collect referenced assets. Check test output for errors.");

		var assetsFile = Path.Combine(projectDir, "obj", "Debug", DotNetCurrent, "resizetizer", "r", "early-optin.assets");
		Assert.True(File.Exists(assetsFile), $"Early opt-in asset output list was not created: {assetsFile}");
		var processedAssets = File.ReadAllLines(assetsFile);
		Assert.NotEmpty(processedAssets);
	}

	static bool WasTargetSkippedAsUpToDate(string binlogPath, string targetName) =>
		new BinLogReader().ReadRecords(binlogPath).Any(record =>
			record.Args is TargetSkippedEventArgs skipped &&
			skipped.TargetName == targetName &&
			skipped.SkipReason == TargetSkipReason.OutputsUpToDate);
}
