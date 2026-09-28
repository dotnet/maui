#nullable enable
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security;
using System.Threading;
using Xunit;

namespace Microsoft.Maui.Resizetizer.Tests;

public class NativeAppleIconTargetsTests
{
	[Fact]
	public void CollectsIconComposerBundleWithoutResizetizer()
	{
		using var project = new TestProject();
		var bundle = project.CreateIconBundle(
			Path.Combine("Native Icon Resources", "Distinctive.ICON"),
			"back.png",
			Path.Combine("Foreground", "front.png"));
		project.WriteProject(CreateProject(bundle, isApple: true));

		var result = project.Run("Capture");

		Assert.True(result.Success, result.Output);
		AssertContains("AppIcon=Distinctive", result.Capture);
		AssertContains("XSAppIconAssets=", result.Capture);
		AssertDoesNotContain("XSAppIconAssets=Assets.xcassets/appicon.appiconset", result.Capture);
		AssertContains("Distinctive.icon/Assets/back.png", Normalize(result.Capture));
		AssertContains("Distinctive.icon/Assets/Foreground/front.png", Normalize(result.Capture));
		AssertContains("Distinctive.icon/icon.json", Normalize(result.Capture));
		AssertDoesNotContain("MauiImage=", result.Capture);
	}

	[Fact]
	public void TracksBundleChildAddDeleteAndRename()
	{
		using var project = new TestProject();
		var bundle = project.CreateIconBundle("Distinctive.icon", "back.png", "front.png");
		project.WriteProject(CreateProject(bundle, isApple: true));

		var first = project.Run("Capture");
		Assert.True(first.Success, first.Output);

		var manifest = Path.Combine(project.Directory, "obj", "mauinativeappleicon.inputs");
		var firstContents = File.ReadAllText(manifest);
		var firstTimestamp = File.GetLastWriteTimeUtc(manifest);
		AssertContains("back.png", firstContents);
		AssertContains("front.png", firstContents);

		Thread.Sleep(1100);
		var second = project.Run("Capture");
		Assert.True(second.Success, second.Output);
		Assert.Equal(firstTimestamp, File.GetLastWriteTimeUtc(manifest));

		File.Delete(Path.Combine(bundle, "Assets", "back.png"));
		File.Move(
			Path.Combine(bundle, "Assets", "front.png"),
			Path.Combine(bundle, "Assets", "renamed.png"));

		var third = project.Run("Capture");
		Assert.True(third.Success, third.Output);

		var thirdContents = File.ReadAllText(manifest);
		AssertDoesNotContain("back.png", thirdContents);
		AssertDoesNotContain("front.png", thirdContents);
		AssertContains("renamed.png", thirdContents);
		Assert.True(File.GetLastWriteTimeUtc(manifest) > firstTimestamp);
	}

	[Fact]
	public void LeavesOrdinaryAndNonAppleMauiIconsOnExistingPath()
	{
		using var appleProject = new TestProject();
		var svg = appleProject.WriteFile("appicon.svg", "<svg xmlns=\"http://www.w3.org/2000/svg\" />");
		appleProject.WriteProject(CreateProject(svg, isApple: true));

		var appleResult = appleProject.Run("Capture");
		Assert.True(appleResult.Success, appleResult.Output);
		AssertContains("MauiImage=", appleResult.Capture);
		AssertContains("appicon.svg;IsAppIcon=True", Normalize(appleResult.Capture));
		AssertContains("XSAppIconAssets=Assets.xcassets/appicon.appiconset", appleResult.Capture);

		using var androidProject = new TestProject();
		var androidBundle = androidProject.CreateIconBundle("appicon.icon", "layer.png");
		androidProject.WriteProject(CreateProject(androidBundle, isApple: false));

		var androidResult = androidProject.Run("Capture");
		Assert.True(androidResult.Success, androidResult.Output);
		AssertContains("appicon.icon;IsAppIcon=True", Normalize(androidResult.Capture));
		AssertDoesNotContain("ImageAsset=", androidResult.Capture);
	}

	[Fact]
	public void InvalidatesAppleCachesWhenSwitchingToOrdinaryIcon()
	{
		using var project = new TestProject();
		var bundle = project.CreateIconBundle("Distinctive.icon", "layer.png");
		project.WriteProject(CreateProject(bundle, isApple: true));

		var nativeResult = project.Run("Capture");
		Assert.True(nativeResult.Success, nativeResult.Output);

		var inputsFile = Path.Combine(project.Directory, "obj", "mauinativeappleicon.inputs");
		var partialManifestCache = project.WriteFile(Path.Combine("obj", "actool-partial.plist"), "native");
		var bundleResourceCache = project.WriteFile(Path.Combine("obj", "actool-bundle-items"), "native");
		Assert.True(File.Exists(inputsFile));

		var svg = project.WriteFile("appicon.svg", "<svg xmlns=\"http://www.w3.org/2000/svg\" />");
		project.WriteProject(CreateProject(svg, isApple: true));

		var ordinaryResult = project.Run("Capture");

		Assert.True(ordinaryResult.Success, ordinaryResult.Output);
		Assert.False(File.Exists(inputsFile));
		Assert.False(File.Exists(partialManifestCache));
		Assert.False(File.Exists(bundleResourceCache));
		AssertContains("appicon.svg;IsAppIcon=True", Normalize(ordinaryResult.Capture));
	}

	[Theory]
	[InlineData("26.4.11514-net11-p4", "requires the .NET Apple workload version")]
	[InlineData("26.5.11720-net11-p6", "must contain an icon.json file")]
	public void ReportsUnsupportedWorkloadAndInvalidBundle(string workloadVersion, string expectedError)
	{
		using var project = new TestProject();
		var bundle = project.CreateDirectory("Broken.icon");
		project.WriteFile(Path.Combine("Broken.icon", "Assets", "layer.png"), "placeholder");
		project.WriteProject(CreateProject(bundle, isApple: true, workloadVersion));

		var result = project.Run("Capture");

		Assert.False(result.Success);
		AssertContains(expectedError, result.Output);
	}

	[Fact]
	public void ReportsMultipleAndMixedMauiIcons()
	{
		using var multipleProject = new TestProject();
		var first = multipleProject.CreateIconBundle("First.icon", "layer.png");
		var second = multipleProject.CreateIconBundle("Second.icon", "layer.png");
		multipleProject.WriteProject(CreateProject(first, isApple: true, additionalMauiIcon: second));

		var multipleResult = multipleProject.Run("Capture");
		Assert.False(multipleResult.Success);
		AssertContains("Only one Apple Icon Composer .icon bundle", multipleResult.Output);

		using var mixedProject = new TestProject();
		var native = mixedProject.CreateIconBundle("Native.icon", "layer.png");
		var svg = mixedProject.WriteFile("appicon.svg", "<svg xmlns=\"http://www.w3.org/2000/svg\" />");
		mixedProject.WriteProject(CreateProject(native, isApple: true, additionalMauiIcon: svg));

		var mixedResult = mixedProject.Run("Capture");
		Assert.False(mixedResult.Success);
		AssertContains("cannot be combined with another MauiIcon", mixedResult.Output);
	}

	[Fact]
	public void ReportsMissingDirectoryEmptyAssetsAndMismatchedAppIcon()
	{
		using var missingProject = new TestProject();
		missingProject.WriteProject(CreateProject(
			Path.Combine(missingProject.Directory, "Missing.icon"),
			isApple: true));

		var missingResult = missingProject.Run("Capture");
		Assert.False(missingResult.Success);
		AssertContains("directory does not exist", missingResult.Output);

		using var emptyProject = new TestProject();
		var emptyBundle = emptyProject.CreateIconBundle("Empty.icon");
		emptyProject.WriteFile(Path.Combine("Empty.icon", "Assets", ".DS_Store"), "finder metadata");
		emptyProject.WriteProject(CreateProject(emptyBundle, isApple: true));

		var emptyResult = emptyProject.Run("Capture");
		Assert.False(emptyResult.Success);
		AssertContains("must contain at least one file in its Assets directory", emptyResult.Output);

		using var mismatchedProject = new TestProject();
		var native = mismatchedProject.CreateIconBundle("Native.icon", "layer.png");
		mismatchedProject.WriteProject(CreateProject(
			native,
			isApple: true,
			appIcon: "Different"));

		var mismatchedResult = mismatchedProject.Run("Capture");
		Assert.False(mismatchedResult.Success);
		AssertContains("does not match the MauiIcon bundle name 'Native'", mismatchedResult.Output);
	}

	[Fact]
	public void CollectsSingleAppleIconComposerBundleFromProjectReference()
	{
		using var project = new TestProject();
		var bundle = project.CreateIconBundle(Path.Combine("Library", "Linked Icons", "Shared.icon"), "layer.png");
		project.WriteFile(
			Path.Combine("Library", "Library.proj"),
			CreateProject(bundle, isApple: true));
		project.WriteProject(CreateProjectReferenceApp(Path.Combine("Library", "Library.proj")));

		var exportResult = project.Run("CaptureExport", Path.Combine("Library", "Library.proj"));
		Assert.True(exportResult.Success, exportResult.Output);
		var exportedIcons = exportResult.Capture
			.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
			.Where(line => line.StartsWith("ExportedMauiItem=MauiIcon;", StringComparison.Ordinal))
			.ToArray();
		Assert.Single(exportedIcons);
		AssertContains("Linked Icons/Shared.icon", Normalize(exportedIcons[0]));

		var appResult = project.Run("Capture");
		Assert.True(appResult.Success, appResult.Output);
		AssertContains("NativeIconCount=1", appResult.Capture);
		AssertContains("AppIcon=Shared", appResult.Capture);
		AssertContains("Shared.icon/Assets/layer.png", Normalize(appResult.Capture));
	}

	static string CreateProject(
		string mauiIcon,
		bool isApple,
		string workloadVersion = "26.5.11720-net11-p6",
		string? additionalMauiIcon = null,
		string? appIcon = null)
	{
		var target = SecurityElement.Escape(
			Path.Combine(AppContext.BaseDirectory, "Microsoft.Maui.Resizetizer.After.targets"));
		var icon = SecurityElement.Escape(mauiIcon);
		var additionalItem = additionalMauiIcon is null
			? string.Empty
			: $"<MauiIcon Include=\"{SecurityElement.Escape(additionalMauiIcon)}\" />";
		var platformProperties = isApple
			? """
			  <_ResizetizerPlatformIsiOS>True</_ResizetizerPlatformIsiOS>
			  <_ResizetizerIsiOSApp>True</_ResizetizerIsiOSApp>
			  """
			: """
			  <_ResizetizerPlatformIsAndroid>True</_ResizetizerPlatformIsAndroid>
			  <_ResizetizerIsAndroidApp>True</_ResizetizerIsAndroidApp>
			  """;

		return $$"""
			<Project>
			  <PropertyGroup>
			    <TargetFramework>net11.0-{{(isApple ? "ios" : "android")}}</TargetFramework>
			    <TargetFrameworkIdentifier>.NETCoreApp</TargetFrameworkIdentifier>
			    <OutputType>Exe</OutputType>
			    <IntermediateOutputPath>obj/</IntermediateOutputPath>
			    <_ACTool_PartialAppManifestCache>obj/actool-partial.plist</_ACTool_PartialAppManifestCache>
			    <_ACTool_BundleResourceCache>obj/actool-bundle-items</_ACTool_BundleResourceCache>
			    <_ShortPackageVersion>{{workloadVersion}}</_ShortPackageVersion>
			    <AppIcon>{{appIcon}}</AppIcon>
			    <_ResizetizerIsCompatibleApp>True</_ResizetizerIsCompatibleApp>
			    <EnableMauiAssetProcessing>false</EnableMauiAssetProcessing>
			    <EnableMauiFontProcessing>false</EnableMauiFontProcessing>
			    <EnableMauiImageProcessing>false</EnableMauiImageProcessing>
			    <EnableMauiSplashScreenProcessing>false</EnableMauiSplashScreenProcessing>
			    {{platformProperties}}
			  </PropertyGroup>
			  <ItemGroup>
			    <MauiIcon Include="{{icon}}" />
			    {{additionalItem}}
			  </ItemGroup>
			  <Import Project="{{target}}" />
			  <Target Name="_ReadAppManifest">
			    <PropertyGroup>
			      <_XSAppIconAssets>Assets.xcassets/appicon.appiconset</_XSAppIconAssets>
			    </PropertyGroup>
			  </Target>
			  <Target Name="Capture" DependsOnTargets="ResizetizeCollectItems;_CollectMauiNativeAppleIcon;_InvalidateMauiNativeAppleIconCache;_InvalidateRemovedMauiNativeAppleIconCache;_ReadAppManifest">
			    <ItemGroup>
			      <_CaptureLine Include="AppIcon=$(AppIcon)" />
			      <_CaptureLine Include="XSAppIconAssets=$(_XSAppIconAssets)" />
			      <_CaptureLine Include="@(MauiImage->'MauiImage=%(Identity);IsAppIcon=%(IsAppIcon)')" />
			      <_CaptureLine Include="@(ImageAsset->'ImageAsset=%(Identity);Link=%(Link)')" />
			    </ItemGroup>
			    <WriteLinesToFile
			      File="capture.txt"
			      Overwrite="true"
			      Lines="@(_CaptureLine)" />
			  </Target>
			  <Target Name="CaptureExport" DependsOnTargets="GetMauiItems">
			    <ItemGroup>
			      <_CaptureLine Include="@(ExportedMauiItem->'ExportedMauiItem=%(ItemGroupName);%(Identity)')" />
			    </ItemGroup>
			    <WriteLinesToFile
			      File="capture.txt"
			      Overwrite="true"
			      Lines="@(_CaptureLine)" />
			  </Target>
			</Project>
			""";
	}

	static string CreateProjectReferenceApp(string projectReference)
	{
		var target = SecurityElement.Escape(
			Path.Combine(AppContext.BaseDirectory, "Microsoft.Maui.Resizetizer.After.targets"));
		var reference = SecurityElement.Escape(projectReference);

		return $$"""
			<Project>
			  <PropertyGroup>
			    <TargetFramework>net11.0-ios</TargetFramework>
			    <TargetFrameworkIdentifier>.NETCoreApp</TargetFrameworkIdentifier>
			    <OutputType>Exe</OutputType>
			    <IntermediateOutputPath>obj/</IntermediateOutputPath>
			    <_ShortPackageVersion>26.5.11720-net11-p6</_ShortPackageVersion>
			    <_ResizetizerPlatformIsiOS>True</_ResizetizerPlatformIsiOS>
			    <_ResizetizerIsiOSApp>True</_ResizetizerIsiOSApp>
			    <_ResizetizerIsCompatibleApp>True</_ResizetizerIsCompatibleApp>
			    <EnableMauiAssetProcessing>false</EnableMauiAssetProcessing>
			    <EnableMauiFontProcessing>false</EnableMauiFontProcessing>
			    <EnableMauiImageProcessing>false</EnableMauiImageProcessing>
			    <EnableMauiSplashScreenProcessing>false</EnableMauiSplashScreenProcessing>
			  </PropertyGroup>
			  <ItemGroup>
			    <ProjectReference Include="{{reference}}" />
			  </ItemGroup>
			  <Import Project="{{target}}" />
			  <Target Name="Capture" DependsOnTargets="ResizetizeCollectItems;_CollectMauiNativeAppleIcon">
			    <PropertyGroup>
			      <_NativeIconCount>@(_MauiNativeAppleIcon->Count())</_NativeIconCount>
			    </PropertyGroup>
			    <ItemGroup>
			      <_CaptureLine Include="NativeIconCount=$(_NativeIconCount)" />
			      <_CaptureLine Include="AppIcon=$(AppIcon)" />
			      <_CaptureLine Include="@(ImageAsset->'ImageAsset=%(Identity);Link=%(Link)')" />
			    </ItemGroup>
			    <WriteLinesToFile
			      File="capture.txt"
			      Overwrite="true"
			      Lines="@(_CaptureLine)" />
			  </Target>
			</Project>
			""";
	}

	static string Normalize(string value) => value.Replace('\\', '/');

	static void AssertContains(string expected, string actual) =>
		Assert.Contains(expected, actual, StringComparison.Ordinal);

	static void AssertDoesNotContain(string expected, string actual) =>
		Assert.DoesNotContain(expected, actual, StringComparison.Ordinal);

	sealed class TestProject : IDisposable
	{
		public TestProject()
		{
			Directory = Path.Combine(Path.GetTempPath(), $"maui-native-icon-{Guid.NewGuid():N}");
			System.IO.Directory.CreateDirectory(Directory);
		}

		public string Directory { get; }

		public string CreateDirectory(string relativePath)
		{
			var path = Path.Combine(Directory, relativePath);
			System.IO.Directory.CreateDirectory(path);
			return path;
		}

		public string CreateIconBundle(string relativePath, params string[] assets)
		{
			var path = CreateDirectory(relativePath);
			WriteFile(Path.Combine(relativePath, "icon.json"), """{"groups":[{"layers":[]}]}""");
			foreach (var asset in assets)
				WriteFile(Path.Combine(relativePath, "Assets", asset), "placeholder");
			return path;
		}

		public string WriteFile(string relativePath, string contents)
		{
			var path = Path.Combine(Directory, relativePath);
			System.IO.Directory.CreateDirectory(Path.GetDirectoryName(path)!);
			File.WriteAllText(path, contents);
			return path;
		}

		public void WriteProject(string contents) => WriteFile("Test.proj", contents);

		public (bool Success, string Output, string Capture) Run(string target, string projectFile = "Test.proj")
		{
			const int timeoutMilliseconds = 60_000;
			var startInfo = new ProcessStartInfo(GetDotNetHost())
			{
				WorkingDirectory = Directory,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
			};
			startInfo.ArgumentList.Add("msbuild");
			startInfo.ArgumentList.Add(projectFile);
			startInfo.ArgumentList.Add($"-t:{target}");
			startInfo.ArgumentList.Add("-v:minimal");

			using var process = Process.Start(startInfo)!;
			var standardOutputTask = process.StandardOutput.ReadToEndAsync();
			var standardErrorTask = process.StandardError.ReadToEndAsync();
			if (!process.WaitForExit(timeoutMilliseconds))
			{
				process.Kill(entireProcessTree: true);
				process.WaitForExit();
				var timedOutOutput = standardOutputTask.GetAwaiter().GetResult();
				var timedOutError = standardErrorTask.GetAwaiter().GetResult();
				Assert.Fail($"MSBuild timed out after {timeoutMilliseconds}ms.{Environment.NewLine}{timedOutOutput}{timedOutError}");
			}

			var combinedOutput =
				standardOutputTask.GetAwaiter().GetResult() +
				standardErrorTask.GetAwaiter().GetResult();
			var capturePath = Path.Combine(Directory, Path.GetDirectoryName(projectFile) ?? string.Empty, "capture.txt");
			var capture = File.Exists(capturePath) ? File.ReadAllText(capturePath) : string.Empty;
			return (process.ExitCode == 0, combinedOutput, capture);
		}

		static string GetDotNetHost()
		{
			var dotnetHost = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");

			return !string.IsNullOrWhiteSpace(dotnetHost) && File.Exists(dotnetHost)
				? dotnetHost
				: "dotnet";
		}

		public void Dispose()
		{
			if (System.IO.Directory.Exists(Directory))
				System.IO.Directory.Delete(Directory, recursive: true);
		}
	}
}
