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
			if (true)
				return; // Skip: "Running Windows templates is only supported on Windows."

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
	public void ChangingExternalBackendPlatformTypeInvalidatesImages()
	{
		SetTestIdentifier("external-backend-platform-invalidation");
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, "ExternalBackend.csproj");
		var imageFile = Path.Combine(projectDir, "image.svg");

		File.WriteAllText(imageFile, BlankSvgContents);
		File.WriteAllText(projectFile,
			$$"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>{{DotNetCurrent}}</TargetFramework>
			    <ResizetizerPlatformType>generic</ResizetizerPlatformType>
			  </PropertyGroup>
			  <ItemGroup>
			    <PackageReference Include="Microsoft.Maui.Resizetizer" Version="{{MauiPackageVersion}}" />
			    <MauiImage Include="image.svg" />
			  </ItemGroup>
			</Project>
			""");

		Assert.True(DotnetInternal.Build(projectFile, "Debug", target: "ResizetizeImages", properties: BuildProps, output: _output),
			"External backend project failed to process generic images. Check test output for errors.");

		var intermediateDir = Path.Combine(projectDir, "obj", "Debug", DotNetCurrent);
		var outputsFile = Path.Combine(intermediateDir, "mauiimage.outputs");
		var stampFile = Path.Combine(intermediateDir, "mauiimage.stamp");
		Assert.True(File.Exists(outputsFile), $"Resizetizer output list does not exist: {outputsFile}");
		Assert.True(File.Exists(stampFile), $"Resizetizer stamp does not exist: {stampFile}");

		var genericImages = File.ReadAllLines(outputsFile);
		Assert.Equal(2, genericImages.Length);
		Assert.Contains(genericImages, path => path.EndsWith("image.png", StringComparison.OrdinalIgnoreCase));
		Assert.Contains(genericImages, path => path.EndsWith("image@2x.png", StringComparison.OrdinalIgnoreCase));

		File.WriteAllText(
			projectFile,
			File.ReadAllText(projectFile).Replace(
				"<ResizetizerPlatformType>generic</ResizetizerPlatformType>",
				"<ResizetizerPlatformType>android</ResizetizerPlatformType>",
				StringComparison.Ordinal));
		File.SetLastWriteTimeUtc(stampFile, DateTime.UtcNow.AddHours(1));
		var invalidationSentinel = File.GetLastWriteTimeUtc(stampFile);

		Assert.True(DotnetInternal.Build(projectFile, "Debug", target: "ResizetizeImages", properties: BuildProps, output: _output),
			"External backend project failed after changing its platform type to android.");
		Assert.True(File.GetLastWriteTimeUtc(stampFile) < invalidationSentinel,
			"ResizetizeImages should rerun when only ResizetizerPlatformType changes.");

		var androidImages = File.ReadAllLines(outputsFile);
		Assert.Equal(5, androidImages.Length);
		Assert.All(androidImages, path =>
		{
			Assert.StartsWith("drawable-", Path.GetFileName(Path.GetDirectoryName(path)), StringComparison.OrdinalIgnoreCase);
			Assert.True(File.Exists(path), $"Processed Android image does not exist: {path}");
		});
		Assert.All(genericImages, path => Assert.False(File.Exists(path), $"Stale generic image still exists: {path}"));

		File.SetLastWriteTimeUtc(stampFile, DateTime.UtcNow.AddMinutes(1));
		var noOpSentinel = File.GetLastWriteTimeUtc(stampFile);
		Assert.True(DotnetInternal.Build(projectFile, "Debug", target: "ResizetizeImages", properties: BuildProps, output: _output),
			"External backend project failed on an unchanged rebuild.");
		Assert.Equal(noOpSentinel, File.GetLastWriteTimeUtc(stampFile));
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
		var resizetizerStampFile = Path.Combine(projectDir, "obj", "Debug", DotNetCurrent, "mauiimage.stamp");
		Assert.True(File.Exists(resizetizerStampFile), $"Resizetizer stamp does not exist: {resizetizerStampFile}");
		var originalStampWriteTime = File.GetLastWriteTimeUtc(resizetizerStampFile);
		File.SetLastWriteTimeUtc(resizetizerStampFile, DateTime.UtcNow.AddMinutes(1));
		var noOpStampWriteTime = File.GetLastWriteTimeUtc(resizetizerStampFile);
		File.Delete(outputsFile);
		Assert.True(DotnetInternal.Build(projectFile, "Debug", target: "VerifyCustomBackendResources", properties: BuildProps, output: _output),
			"Custom backend project failed on incremental rebuild. Check test output for errors.");
		Assert.Equal(noOpStampWriteTime, File.GetLastWriteTimeUtc(resizetizerStampFile));
		File.SetLastWriteTimeUtc(resizetizerStampFile, originalStampWriteTime);

		var processedImagesIncremental = File.ReadAllLines(outputsFile);
		Assert.Equal(2, processedImagesIncremental.Length);
		Assert.DoesNotContain(processedImagesIncremental, path => path.EndsWith(".items", StringComparison.OrdinalIgnoreCase));

		// A partial cache restore can lose only the persisted output list while leaving
		// the stamp and generated images. The missing list must invalidate the target,
		// rerun image processing, and restore the processed-image contract.
		var resizetizerOutputsFile = Path.Combine(projectDir, "obj", "Debug", DotNetCurrent, "mauiimage.outputs");
		Assert.True(File.Exists(resizetizerOutputsFile), $"Resizetizer output list does not exist: {resizetizerOutputsFile}");
		File.SetLastWriteTimeUtc(resizetizerStampFile, DateTime.UtcNow.AddHours(1));
		var invalidationSentinel = File.GetLastWriteTimeUtc(resizetizerStampFile);
		File.Delete(resizetizerOutputsFile);
		File.Delete(outputsFile);

		Assert.True(DotnetInternal.Build(projectFile, "Debug", target: "VerifyCustomBackendResources", properties: BuildProps, output: _output),
			"Custom backend project failed after deleting only the persisted image output list.");
		Assert.True(File.GetLastWriteTimeUtc(resizetizerStampFile) < invalidationSentinel,
			"ResizetizeImages should rerun when the persisted output list is missing.");
		Assert.True(File.Exists(resizetizerOutputsFile), "Resizetizer output list was not restored.");
		Assert.Equal(2, File.ReadAllLines(outputsFile).Length);

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
			    <Error Condition="'$(ExpectFontPlist)' == 'true' And '@(PartialAppManifest)' == ''" Text="iOS font processing must expose the generated partial app manifest." />
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
		File.SetLastWriteTimeUtc(fontStampFile, DateTime.UtcNow.AddHours(1));
		var noOpFontStampWriteTime = File.GetLastWriteTimeUtc(fontStampFile);
		File.Delete(fontsFile);
		Assert.True(DotnetInternal.Build(projectFile, "Debug", target: "VerifyCustomBackendResources", properties: BuildProps, output: _output),
			"Custom backend project failed on a no-op font rebuild. Check test output for errors.");
		Assert.Equal(noOpFontStampWriteTime, File.GetLastWriteTimeUtc(fontStampFile));
		Assert.True(File.Exists(fontsFile), "Custom backend font output list was not recreated on the no-op rebuild.");
		Assert.Single(File.ReadAllLines(fontsFile));

		// A partial cache restore can retain the future-dated stamp while losing a copied font.
		// The guard must invalidate the stamp, rerun ProcessMauiFonts, and restore both the
		// physical copy and the processed-font contract exposed to the backend.
		File.Delete(processedFontPath);
		File.Delete(fontsFile);
		Assert.True(DotnetInternal.Build(projectFile, "Debug", target: "VerifyCustomBackendResources", properties: BuildProps, output: _output),
			"Custom backend project failed after deleting only a copied processed font.");
		Assert.True(File.GetLastWriteTimeUtc(fontStampFile) < noOpFontStampWriteTime,
			"ProcessMauiFonts should rerun when an expected copied font is missing.");
		Assert.True(File.Exists(processedFontPath), $"Missing processed font was not restored: {processedFontPath}");
		Assert.Equal(processedFont, Assert.Single(File.ReadAllLines(fontsFile)));

		// iOS font processing also produces MauiInfo.plist outside the target's Outputs.
		// A partial cache restore that loses only this side artifact must invalidate the
		// retained stamp and restore both the file and PartialAppManifest contract.
		var iosFontBuildProps = BuildProps;
		iosFontBuildProps.Add("_ResizetizerIsiOSApp=true");
		iosFontBuildProps.Add("ExpectFontPlist=true");
		File.Delete(fontStampFile);
		Assert.True(DotnetInternal.Build(projectFile, "Debug", target: "VerifyCustomBackendResources", properties: iosFontBuildProps, output: _output),
			"Custom backend project failed to bootstrap simulated iOS font registration.");
		var fontPlistFile = Path.Combine(Path.GetDirectoryName(processedFontPath)!, "MauiInfo.plist");
		Assert.True(File.Exists(fontPlistFile), $"iOS font partial manifest was not created: {fontPlistFile}");

		File.SetLastWriteTimeUtc(fontStampFile, DateTime.UtcNow.AddHours(1));
		var plistRecoverySentinel = File.GetLastWriteTimeUtc(fontStampFile);
		File.Delete(fontPlistFile);
		Assert.True(DotnetInternal.Build(projectFile, "Debug", target: "VerifyCustomBackendResources", properties: iosFontBuildProps, output: _output),
			"Custom backend project failed after deleting only the iOS font partial manifest.");
		Assert.True(File.GetLastWriteTimeUtc(fontStampFile) < plistRecoverySentinel,
			"ProcessMauiFonts should rerun when the iOS font partial manifest is missing.");
		Assert.True(File.Exists(fontPlistFile), $"Missing iOS font partial manifest was not restored: {fontPlistFile}");

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

	[Theory]
	[InlineData("maui", "mauilib", true)]
	[InlineData("maui", "mauilib", false)]
	public void AdditionalPropertiesExcludesImage(string id, string libid, bool unpackaged)
	{
		SetTestIdentifier(id, libid, unpackaged);
		// TODO: fix the tests as they have been disabled too long!
		if (!TestEnvironment.IsWindows)
			if (true)
				return; // Skip: "Running Windows templates is only supported on Windows."

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
	public void AdditionalPropertiesSelectsImageInLibrary(string id, string libid, bool unpackaged)
	{
		SetTestIdentifier(id, libid, unpackaged);
		// TODO: fix the tests as they have been disabled too long!
		if (!TestEnvironment.IsWindows)
			if (true)
				return; // Skip: "Running Windows templates is only supported on Windows."

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

		// build
		Assert.True(DotnetInternal.Build(appFile, "Debug", properties: BuildProps, output: _output),
			$"Project {Path.GetFileName(appFile)} failed to build. Check test output/attachments for errors.");

		// assert - alternate_image should be collected (property was propagated)
		Assert.True(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-android\\resizetizer\\r\\drawable-mdpi\\alternate_image.png")),
			"Android was missing alternate_image — AdditionalProperties was not propagated.");
		// assert - default_image should NOT be collected (it was excluded by the property)
		Assert.False(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-android\\resizetizer\\r\\drawable-mdpi\\default_image.png")),
			"Android should NOT have default_image — AdditionalProperties should have selected the alternate.");
		if (TestEnvironment.IsWindows)
		{
			Assert.True(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-windows10.0.19041.0\\win-x64\\resizetizer\\r\\alternate_image.scale-100.png")),
				"Windows was missing alternate_image — AdditionalProperties was not propagated.");
			Assert.False(File.Exists(Path.Combine(appDir, $"obj\\Debug\\{DotNetCurrent}-windows10.0.19041.0\\win-x64\\resizetizer\\r\\default_image.scale-100.png")),
				"Windows should NOT have default_image — AdditionalProperties should have selected the alternate.");
		}
	}
}
