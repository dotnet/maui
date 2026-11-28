using System.Security;

namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class WindowsApplicationArtifactsTargetsTest : BaseBuildTest
{
	public WindowsApplicationArtifactsTargetsTest(IntegrationTestFixture fixture, ITestOutputHelper output)
		: base(fixture, output)
	{
	}

	static string TargetsFile => Path.Combine(
		TestEnvironment.GetMauiDirectory(),
		"src",
		"Core",
		"src",
		"nuget",
		"buildTransitive",
		"WinUI.ApplicationArtifacts.targets");

	const string ArtifactFormat =
		"%(FullPath)|%(PackageFormat)|%(IsDirectory)|%(Signed)|%(EntryPoint)|%(RuntimeIdentifier)|%(PlatformName)|%(ApplicationId)|%(ApplicationTitle)|%(ApplicationName)|%(ApplicationDisplayVersion)|%(ApplicationVersion)|%(Extended)";

	[Fact]
	public void PackagedBuildReportsOnlyFinalPackages()
	{
		SetTestIdentifier(nameof(PackagedBuildReportsOnlyFinalPackages));
		var packageDir = Directory.CreateDirectory(Path.Combine(TestDirectory, "AppPackages")).FullName;
		var packageTestDir = Directory.CreateDirectory(Path.Combine(packageDir, "ArtifactApp_2.3.4.5_x64_Test")).FullName;
		var targetDir = Directory.CreateDirectory(Path.Combine(TestDirectory, "bin", "Debug", "net11.0-windows")).FullName;
		var uploadDir = Directory.CreateDirectory(Path.Combine(targetDir, "Upload")).FullName;
		var intermediateDir = Directory.CreateDirectory(Path.Combine(TestDirectory, "obj", "Debug")).FullName;

		var primary = CreateFile(packageTestDir, "ArtifactApp_2.3.4.5_x64.msix");
		var bundle = CreateFile(packageTestDir, "ArtifactApp_2.3.4.5_x64.msixbundle");
		var uploadPackage = CreateFile(uploadDir, "ArtifactApp_2.3.4.5_x64.msix");
		var storeUpload = CreateFile(packageDir, "ArtifactApp_2.3.4.5_x64.msixupload");
		var appInstaller = CreateFile(packageDir, "ArtifactApp.appinstaller");
		var symbols = CreateFile(packageTestDir, "ArtifactApp_2.3.4.5_x64.appxsym");
		var certificate = CreateFile(packageTestDir, "ArtifactApp.cer");
		var installScript = CreateFile(packageTestDir, "Install.ps1");
		var dependency = CreateFile(Path.Combine(packageTestDir, "Dependencies", "x64"), "Framework.msix");
		var support = CreateFile(packageDir, "install.json");
		var intermediate = CreateFile(intermediateDir, "intermediate.msix");

		var manifest = Path.Combine(targetDir, "AppxManifest.xml");
		File.WriteAllText(manifest, """
			<?xml version="1.0" encoding="utf-8"?>
			<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10">
			  <Identity Name="11111111-2222-3333-4444-555555555555" Publisher="CN=Test" Version="2.3.4.5" />
			  <Properties>
			    <DisplayName>ArtifactApp</DisplayName>
			  </Properties>
			  <Applications>
			    <Application Id="App" Executable="ArtifactApp.exe" EntryPoint="ArtifactApp.App" />
			  </Applications>
			</Package>
			""");

		var artifactsFile = Path.Combine(TestDirectory, "application-artifacts.txt");
		var projectFile = Path.Combine(TestDirectory, "Packaged.proj");

		File.WriteAllText(projectFile, $"""
			<Project>
			  <PropertyGroup>
			    <TargetFramework>net11.0-windows10.0.19041.0</TargetFramework>
			    <OutputType>WinExe</OutputType>
			    <BuildingProject>false</BuildingProject>
			    <MicrosoftWindowsAppSDKPackageDir>{Escape(packageDir)}</MicrosoftWindowsAppSDKPackageDir>
			    <WindowsPackageType>MSIX</WindowsPackageType>
			    <TargetDir>{Escape(WithDirectorySeparator(targetDir))}</TargetDir>
			    <OutDir>{Escape(WithDirectorySeparator(targetDir))}</OutDir>
			    <IntermediateOutputPath>{Escape(WithDirectorySeparator(intermediateDir))}</IntermediateOutputPath>
			    <AppxPackageDir>{Escape(WithDirectorySeparator(packageDir))}</AppxPackageDir>
			    <AppxPackageTestDir>{Escape(WithDirectorySeparator(packageTestDir))}</AppxPackageTestDir>
			    <AppxUploadPackageDir>{Escape(WithDirectorySeparator(uploadDir))}</AppxUploadPackageDir>
			    <AppxPackageOutput>{Escape(primary)}</AppxPackageOutput>
			    <AppxBundleOutput>{Escape(bundle)}</AppxBundleOutput>
			    <AppxUploadPackageOutput>{Escape(uploadPackage)}</AppxUploadPackageOutput>
			    <AppxStoreContainer>{Escape(storeUpload)}</AppxStoreContainer>
			    <FinalAppxManifestName>{Escape(manifest)}</FinalAppxManifestName>
			    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
			    <AppxPackageSigningEnabled>true</AppxPackageSigningEnabled>
			    <AssemblyName>ArtifactApp</AssemblyName>
			    <GetApplicationArtifactsDependsOn>ExtendCollectedArtifacts</GetApplicationArtifactsDependsOn>
			  </PropertyGroup>
			  <Target Name="Build">
			    <PropertyGroup>
			      <BuildingProject>true</BuildingProject>
			    </PropertyGroup>
			    <ItemGroup>
			      <FinalAppxPackageItem Include="{Escape(primary)}" />
			      <AppInstallerFilePath Include="{Escape(appInstaller)}" />
			      <InstallerFileWrites Include="{Escape(primary)};{Escape(certificate)};{Escape(dependency)};{Escape(appInstaller)};{Escape(support)}" />
			      <PackagingFileWrites Include="{Escape(primary)};{Escape(bundle)};{Escape(storeUpload)};{Escape(symbols)};{Escape(installScript)};{Escape(dependency)};{Escape(intermediate)};{Escape(uploadPackage)}" />
			      <AllBuiltSideloadPackages Include="{Escape(primary)};{Escape(bundle)}" />
			    </ItemGroup>
			  </Target>
			  <Target Name="_ComputeAppxPackageOutput" />
			  <Target Name="GenerateMsixPackage" DependsOnTargets="Build" />
			  <Import Project="{Escape(TargetsFile)}" />
			  <Target Name="ExtendCollectedArtifacts">
			    <ItemGroup>
			      <ApplicationArtifact Update="@(ApplicationArtifact)" Extended="true" />
			    </ItemGroup>
			  </Target>
			  <Target Name="WriteApplicationArtifacts" DependsOnTargets="GetApplicationArtifacts">
			    <WriteLinesToFile File="{Escape(artifactsFile)}" Lines="@(ApplicationArtifact->'{ArtifactFormat}')" Overwrite="true" />
			  </Target>
			</Project>
			""");

		Assert.True(
			DotnetInternal.Run("msbuild", $"\"{projectFile}\" -t:WriteApplicationArtifacts -v:minimal", output: _output),
			"Unable to execute the packaged Windows ApplicationArtifact fixture.");

		var artifacts = File.ReadAllLines(artifactsFile).Select(ArtifactRecord.Parse).ToArray();

		Assert.Equal(4, artifacts.Length);
		AssertFormat(primary, "msix");
		AssertFormat(bundle, "msixbundle");
		AssertFormat(storeUpload, "msixupload");
		AssertFormat(appInstaller, "appinstaller");

		Assert.All(artifacts, artifact =>
		{
			Assert.Equal("false", artifact.IsDirectory, ignoreCase: true);
			Assert.Equal("true", artifact.Signed, ignoreCase: true);
			Assert.Equal("ArtifactApp.exe", artifact.EntryPoint);
			Assert.Equal("win-x64", artifact.RuntimeIdentifier);
			Assert.Equal("Windows", artifact.PlatformName);
			Assert.Equal("11111111-2222-3333-4444-555555555555", artifact.ApplicationId);
			Assert.Equal("ArtifactApp", artifact.ApplicationTitle);
			Assert.Equal("ArtifactApp", artifact.ApplicationName);
			Assert.Equal("2.3.4", artifact.ApplicationDisplayVersion);
			Assert.Equal("5", artifact.ApplicationVersion);
			Assert.Equal("true", artifact.Extended);
		});

		void AssertFormat(string path, string format)
		{
			var artifact = Assert.Single(artifacts, candidate => SamePath(candidate.Path, path));
			Assert.Equal(format, artifact.PackageFormat);
		}
	}

	[Theory]
	[InlineData("net11.0", "WinExe", true, true, false, true)]
	[InlineData("net11.0-windows10.0.19041.0", "Library", true, true, false, true)]
	[InlineData("net11.0-windows10.0.19041.0", "WinExe", false, true, false, true)]
	[InlineData("net11.0-windows10.0.19041.0", "WinExe", true, false, false, true)]
	[InlineData("net11.0-windows10.0.19041.0", "WinExe", true, true, true, true)]
	[InlineData("net11.0-windows10.0.19041.0", "WinExe", true, true, false, false)]
	public void TargetsAreInertOutsideRealWindowsAppSdkApplications(
		string targetFramework,
		string outputType,
		bool hasWindowsAppSdk,
		bool enabled,
		bool designTimeBuild,
		bool buildingProject)
	{
		SetTestIdentifier(targetFramework, outputType, hasWindowsAppSdk, enabled, designTimeBuild, buildingProject);
		var artifactsFile = Path.Combine(TestDirectory, "application-artifacts.txt");
		var projectFile = Path.Combine(TestDirectory, "Inert.proj");

		File.WriteAllText(projectFile, $"""
			<Project>
			  <PropertyGroup>
			    <TargetFramework>{targetFramework}</TargetFramework>
			    <OutputType>{outputType}</OutputType>
			    <BuildingProject>{buildingProject}</BuildingProject>
			    <DesignTimeBuild>{designTimeBuild}</DesignTimeBuild>
			    <EnableWindowsApplicationArtifacts>{enabled}</EnableWindowsApplicationArtifacts>
			    <MicrosoftWindowsAppSDKPackageDir>{(hasWindowsAppSdk ? Escape(TestDirectory) : string.Empty)}</MicrosoftWindowsAppSDKPackageDir>
			    <WindowsPackageType>None</WindowsPackageType>
			    <TargetDir>{Escape(WithDirectorySeparator(TestDirectory))}</TargetDir>
			  </PropertyGroup>
			  <Target Name="Build" />
			  <Import Project="{Escape(TargetsFile)}" />
			  <Target Name="WriteApplicationArtifacts" DependsOnTargets="GetApplicationArtifacts">
			    <WriteLinesToFile File="{Escape(artifactsFile)}" Lines="@(ApplicationArtifact)" Overwrite="true" />
			  </Target>
			</Project>
			""");

		Assert.True(
			DotnetInternal.Run("msbuild", $"\"{projectFile}\" -t:WriteApplicationArtifacts -v:minimal", output: _output),
			"Unable to execute the inert Windows ApplicationArtifact fixture.");
		Assert.True(!File.Exists(artifactsFile) || new FileInfo(artifactsFile).Length == 0);
	}

	[Fact]
	public void SparsePackageReportsTheFinalMsix()
	{
		SetTestIdentifier(nameof(SparsePackageReportsTheFinalMsix));
		var targetDir = Directory.CreateDirectory(Path.Combine(TestDirectory, "bin")).FullName;
		var sparsePackage = CreateFile(targetDir, "SparseApp.msix");
		var artifactsFile = Path.Combine(TestDirectory, "application-artifacts.txt");
		var projectFile = Path.Combine(TestDirectory, "Sparse.proj");

		File.WriteAllText(projectFile, $"""
			<Project>
			  <PropertyGroup>
			    <TargetFramework>net11.0-windows10.0.19041.0</TargetFramework>
			    <OutputType>WinExe</OutputType>
			    <BuildingProject>false</BuildingProject>
			    <MicrosoftWindowsAppSDKPackageDir>{Escape(TestDirectory)}</MicrosoftWindowsAppSDKPackageDir>
			    <WindowsPackageType>Sparse</WindowsPackageType>
			    <TargetDir>{Escape(WithDirectorySeparator(targetDir))}</TargetDir>
			    <AppxPackageOutput>{Escape(sparsePackage)}</AppxPackageOutput>
			    <AssemblyName>SparseApp</AssemblyName>
			  </PropertyGroup>
			  <Target Name="Build">
			    <PropertyGroup>
			      <BuildingProject>true</BuildingProject>
			    </PropertyGroup>
			  </Target>
			  <Target Name="_ComputeAppxPackageOutput" />
			  <Target Name="GenerateMsixPackage" />
			  <Import Project="{Escape(TargetsFile)}" />
			  <Target Name="WriteApplicationArtifacts" DependsOnTargets="GetApplicationArtifacts">
			    <WriteLinesToFile File="{Escape(artifactsFile)}" Lines="@(ApplicationArtifact->'{ArtifactFormat}')" Overwrite="true" />
			  </Target>
			</Project>
			""");

		Assert.True(
			DotnetInternal.Run("msbuild", $"\"{projectFile}\" -t:WriteApplicationArtifacts -v:minimal", output: _output),
			"Unable to execute the sparse Windows ApplicationArtifact fixture.");

		var artifact = ArtifactRecord.Parse(Assert.Single(File.ReadAllLines(artifactsFile)));
		Assert.True(SamePath(sparsePackage, artifact.Path));
		Assert.Equal("msix", artifact.PackageFormat);
		Assert.Equal("false", artifact.IsDirectory, ignoreCase: true);
		Assert.Equal("false", artifact.Signed, ignoreCase: true);
		Assert.Equal("Windows", artifact.PlatformName);
	}

	[Fact]
	public void UnpackagedBuildAndPublishReportTheirOutputDirectory()
	{
		SetTestIdentifier(nameof(UnpackagedBuildAndPublishReportTheirOutputDirectory));
		var targetDir = Directory.CreateDirectory(Path.Combine(TestDirectory, "bin")).FullName;
		var publishDir = Directory.CreateDirectory(Path.Combine(targetDir, "publish")).FullName;
		_ = CreateFile(targetDir, "UnpackagedApp.exe");
		_ = CreateFile(publishDir, "UnpackagedApp.exe");
		var buildArtifactsFile = Path.Combine(TestDirectory, "build-application-artifacts.txt");
		var publishArtifactsFile = Path.Combine(TestDirectory, "publish-application-artifacts.txt");
		var projectFile = Path.Combine(TestDirectory, "Unpackaged.proj");

		File.WriteAllText(projectFile, $"""
			<Project>
			  <PropertyGroup>
			    <TargetFramework>net11.0-windows10.0.19041.0</TargetFramework>
			    <OutputType>WinExe</OutputType>
			    <BuildingProject>true</BuildingProject>
			    <MicrosoftWindowsAppSDKPackageDir>{Escape(TestDirectory)}</MicrosoftWindowsAppSDKPackageDir>
			    <WindowsPackageType>None</WindowsPackageType>
			    <TargetDir>{Escape(WithDirectorySeparator(targetDir))}</TargetDir>
			    <PublishDir>{Escape(WithDirectorySeparator(publishDir))}</PublishDir>
			    <TargetName>UnpackagedApp</TargetName>
			    <AssemblyName>UnpackagedApp</AssemblyName>
			    <RuntimeIdentifier>win-arm64</RuntimeIdentifier>
			    <ApplicationId>com.example.unpackaged</ApplicationId>
			    <ApplicationTitle>Unpackaged App</ApplicationTitle>
			    <ApplicationDisplayVersion>1.2.3</ApplicationDisplayVersion>
			    <ApplicationVersion>4</ApplicationVersion>
			  </PropertyGroup>
			  <Target Name="Build" />
			  <Target Name="PreparePublishOutput" />
			  <Import Project="{Escape(TargetsFile)}" />
			  <Target Name="Publish" DependsOnTargets="PreparePublishOutput" />
			  <Target Name="WriteBuildApplicationArtifacts" DependsOnTargets="GetApplicationArtifacts">
			    <WriteLinesToFile File="{Escape(buildArtifactsFile)}" Lines="@(ApplicationArtifact->'{ArtifactFormat}')" Overwrite="true" />
			  </Target>
			  <Target Name="WritePublishedApplicationArtifacts" AfterTargets="Publish">
			    <WriteLinesToFile File="{Escape(publishArtifactsFile)}" Lines="@(ApplicationArtifact->'{ArtifactFormat}')" Overwrite="true" />
			  </Target>
			</Project>
			""");

		Assert.True(
			DotnetInternal.Run("msbuild", $"\"{projectFile}\" -t:WriteBuildApplicationArtifacts;Publish -v:minimal", output: _output),
			"Unable to collect unpackaged build and publish artifacts in one MSBuild invocation.");
		AssertDirectoryArtifact(publishArtifactsFile, publishDir);

		File.Delete(buildArtifactsFile);
		File.Delete(publishArtifactsFile);

		Assert.True(
			DotnetInternal.Run("msbuild", $"\"{projectFile}\" -t:WriteBuildApplicationArtifacts -v:minimal", output: _output),
			"Unable to collect unpackaged build artifacts.");
		AssertDirectoryArtifact(buildArtifactsFile, targetDir);

		Assert.True(
			DotnetInternal.Run("msbuild", $"\"{projectFile}\" -t:Publish -v:minimal", output: _output),
			"Unable to collect unpackaged publish artifacts.");
		AssertDirectoryArtifact(publishArtifactsFile, publishDir);

		static void AssertDirectoryArtifact(string artifactsFile, string expectedDirectory)
		{
			var artifact = ArtifactRecord.Parse(Assert.Single(File.ReadAllLines(artifactsFile)));
			Assert.True(SamePath(expectedDirectory, artifact.Path), $"Expected '{expectedDirectory}' but got '{artifact.Path}'.");
			Assert.Equal("directory", artifact.PackageFormat);
			Assert.Equal("true", artifact.IsDirectory, ignoreCase: true);
			Assert.Equal("false", artifact.Signed, ignoreCase: true);
			Assert.Equal("UnpackagedApp.exe", artifact.EntryPoint);
			Assert.Equal("win-arm64", artifact.RuntimeIdentifier);
			Assert.Equal("Windows", artifact.PlatformName);
			Assert.Equal("com.example.unpackaged", artifact.ApplicationId);
			Assert.Equal("Unpackaged App", artifact.ApplicationTitle);
			Assert.Equal("1.2.3", artifact.ApplicationDisplayVersion);
			Assert.Equal("4", artifact.ApplicationVersion);
		}
	}

	static bool SamePath(string left, string right) =>
		string.Equals(
			Path.TrimEndingDirectorySeparator(Path.GetFullPath(left)),
			Path.TrimEndingDirectorySeparator(Path.GetFullPath(right)),
			StringComparison.OrdinalIgnoreCase);

	static string CreateFile(string directory, string fileName)
	{
		Directory.CreateDirectory(directory);
		var path = Path.Combine(directory, fileName);
		File.WriteAllText(path, fileName);
		return path;
	}

	static string WithDirectorySeparator(string path) =>
		path.EndsWith(Path.DirectorySeparatorChar, StringComparison.Ordinal) ? path : path + Path.DirectorySeparatorChar;

	static string Escape(string value) => SecurityElement.Escape(value) ?? string.Empty;

	sealed record ArtifactRecord(
		string Path,
		string PackageFormat,
		string IsDirectory,
		string Signed,
		string EntryPoint,
		string RuntimeIdentifier,
		string PlatformName,
		string ApplicationId,
		string ApplicationTitle,
		string ApplicationName,
		string ApplicationDisplayVersion,
		string ApplicationVersion,
		string Extended)
	{
		public static ArtifactRecord Parse(string line)
		{
			var fields = line.Split('|');
			Assert.Equal(13, fields.Length);
			return new(fields[0], fields[1], fields[2], fields[3], fields[4], fields[5], fields[6],
				fields[7], fields[8], fields[9], fields[10], fields[11], fields[12]);
		}
	}
}
