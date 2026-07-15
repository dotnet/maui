using System.IO.Compression;
using System.Security;
using System.Xml.Linq;

namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class WindowsApplicationArtifactsPackageTest : BaseBuildTest
{
	const string PackageId = "Microsoft.Maui.ApplicationArtifacts.Windows";

	public WindowsApplicationArtifactsPackageTest(IntegrationTestFixture fixture, ITestOutputHelper output)
		: base(fixture, output)
	{
	}

	[Fact]
	public void PackageContainsOnlyNeutralBuildAssets()
	{
		SetTestIdentifier(nameof(PackageContainsOnlyNeutralBuildAssets));
		var packagePath = FindPackage(PackageId);

		using var archive = ZipFile.OpenRead(packagePath);
		var entries = archive.Entries.Select(entry => entry.FullName).ToArray();

		Assert.Contains("buildTransitive/Microsoft.Maui.ApplicationArtifacts.Windows.targets", entries);
		Assert.DoesNotContain(entries, entry =>
			entry.EndsWith(".dll", StringComparison.OrdinalIgnoreCase) ||
			entry.StartsWith("lib/", StringComparison.OrdinalIgnoreCase) ||
			entry.StartsWith("runtimes/", StringComparison.OrdinalIgnoreCase) ||
			entry.StartsWith("tools/", StringComparison.OrdinalIgnoreCase) ||
			entry.StartsWith("analyzers/", StringComparison.OrdinalIgnoreCase));
		Assert.Single(entries.Where(entry => entry.StartsWith("buildTransitive/", StringComparison.OrdinalIgnoreCase)));

		var nuspecEntry = Assert.Single(archive.Entries.Where(entry => entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase)));
		using var nuspecStream = nuspecEntry.Open();
		var nuspec = XDocument.Load(nuspecStream);
		var ns = nuspec.Root!.Name.Namespace;
		var packageType = Assert.Single(nuspec.Descendants(ns + "packageType"));
		Assert.Equal("Dependency", (string?)packageType.Attribute("name"));
	}

	[Fact]
	public void CorePackageDependsOnApplicationArtifactsOnlyForWindows()
	{
		SetTestIdentifier(nameof(CorePackageDependsOnApplicationArtifactsOnlyForWindows));
		if (!TestEnvironment.IsWindows)
		{
			if (true)
				return; // Skip: "Packing the Windows Core target framework is only supported on Windows CI."
		}

		var coreProject = Path.Combine(TestEnvironment.GetMauiDirectory(), "src", "Core", "src", "Core.csproj");
		Assert.True(
			DotnetInternal.Run(
				"pack",
				$"\"{coreProject}\" -c Debug --no-build --no-restore -p:Packing=true -p:EnableWindowsTargeting=true -p:IncludeBuildOutput=false -p:NoWarn=NU5127%3BNU5128 -p:PackageOutputPath=\"{TestDirectory}\" -v:minimal",
				output: _output),
			"Unable to generate the Core package dependency graph.");
		var packagePath = Path.Combine(TestDirectory, $"Microsoft.Maui.Core.{MauiPackageVersion}.nupkg");
		Assert.True(File.Exists(packagePath), $"The expected Core package was not created at '{packagePath}'.");

		using var archive = ZipFile.OpenRead(packagePath);
		var nuspecEntry = Assert.Single(archive.Entries.Where(entry => entry.FullName.EndsWith(".nuspec", StringComparison.OrdinalIgnoreCase)));
		using var nuspecStream = nuspecEntry.Open();
		var nuspec = XDocument.Load(nuspecStream);
		var ns = nuspec.Root!.Name.Namespace;
		var dependencyGroups = nuspec
			.Descendants(ns + "group")
			.Where(group => group.Elements(ns + "dependency").Any(dependency =>
				string.Equals((string?)dependency.Attribute("id"), PackageId, StringComparison.Ordinal)))
			.ToArray();

		Assert.NotEmpty(dependencyGroups);
		Assert.All(dependencyGroups, group =>
		{
			Assert.Contains("windows", (string?)group.Attribute("targetFramework") ?? string.Empty, StringComparison.OrdinalIgnoreCase);
			var dependency = Assert.Single(group.Elements(ns + "dependency").Where(candidate =>
				string.Equals((string?)candidate.Attribute("id"), PackageId, StringComparison.Ordinal)));
			Assert.Equal("All", (string?)dependency.Attribute("include"), ignoreCase: true);
			Assert.Null(dependency.Attribute("exclude"));
		});
	}

	[Fact]
	public void TargetsClassifyAndDeduplicateAuthoritativeOutputs()
	{
		SetTestIdentifier(nameof(TargetsClassifyAndDeduplicateAuthoritativeOutputs));
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
		var landingPage = CreateFile(packageDir, "index.html");
		var support = CreateFile(packageDir, "install.json");
		var appInstallerSupport = CreateFile(packageDir, "appinstaller-data.xml");
		var intermediate = CreateFile(intermediateDir, "intermediate.msix");
		_ = CreateFile(targetDir, "loose-payload.dll");

		var manifest = Path.Combine(targetDir, "AppxManifest.xml");
		File.WriteAllText(manifest, """
			<?xml version="1.0" encoding="utf-8"?>
			<Package xmlns="http://schemas.microsoft.com/appx/manifest/foundation/windows10">
			  <Identity Name="11111111-2222-3333-4444-555555555555" Publisher="CN=Test" Version="2.3.4.5" />
			  <Properties>
			    <DisplayName>ms-resource:ArtifactApp</DisplayName>
			  </Properties>
			  <Applications>
			    <Application Id="App" Executable="ArtifactApp.exe" EntryPoint="ArtifactApp.App" />
			  </Applications>
			</Package>
			""");

		var artifactsFile = Path.Combine(TestDirectory, "application-artifacts.txt");
		var projectFile = Path.Combine(TestDirectory, "ArtifactClassification.proj");
		var targetsFile = Path.Combine(
			TestEnvironment.GetMauiDirectory(),
			"src",
			"Workload",
			PackageId,
			"buildTransitive",
			$"{PackageId}.targets");

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
			    <AppxSymbolPackageOutput>{Escape(symbols)}</AppxSymbolPackageOutput>
			    <AppxPackagePublicKeyFile>{Escape(certificate)}</AppxPackagePublicKeyFile>
			    <LandingPagePath>{Escape(landingPage)}</LandingPagePath>
			    <FinalAppxManifestName>{Escape(manifest)}</FinalAppxManifestName>
			    <PackageArchitecture>x64</PackageArchitecture>
			    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
			    <AppxPackageSigningEnabled>true</AppxPackageSigningEnabled>
			    <AppxBundlePlatforms>x64_arm64</AppxBundlePlatforms>
			    <AssemblyName>ArtifactApp</AssemblyName>
			    <GetApplicationArtifactsDependsOn>StampCollectedArtifacts</GetApplicationArtifactsDependsOn>
			  </PropertyGroup>
			  <Target Name="Build">
			    <PropertyGroup>
			      <BuildingProject>true</BuildingProject>
			    </PropertyGroup>
			    <ItemGroup>
			      <FinalAppxPackageItem Include="{Escape(primary)}" />
			      <AppInstallerFilePath Include="{Escape(appInstaller)}" />
			      <InstallerFileWrites Include="{Escape(primary)};{Escape(certificate)};{Escape(dependency)};{Escape(landingPage)};{Escape(appInstaller)};{Escape(support)}" />
			      <PackagingFileWrites Include="{Escape(primary)};{Escape(bundle)};{Escape(storeUpload)};{Escape(symbols)};{Escape(installScript)};{Escape(dependency)};{Escape(support)};{Escape(intermediate)}" />
			      <AllBuiltSideloadPackages Include="{Escape(primary)};{Escape(bundle)}" />
			      <AllBuiltUploadPackages Include="{Escape(uploadPackage)}" />
			      <AppInstallerFileWrites Include="{Escape(appInstallerSupport)}" />
			    </ItemGroup>
			  </Target>
			  <Target Name="_ComputeAppxPackageOutput" />
			  <Target Name="GenerateMsixPackage" DependsOnTargets="Build" />
			  <Import Project="{Escape(targetsFile)}" />
			  <Target Name="StampCollectedArtifacts">
			    <ItemGroup>
			      <ApplicationArtifact Update="@(ApplicationArtifact)">
			        <BundlePlatforms>extended-%(ApplicationArtifact.BundlePlatforms)</BundlePlatforms>
			      </ApplicationArtifact>
			    </ItemGroup>
			  </Target>
			  <Target Name="WriteApplicationArtifacts" DependsOnTargets="GetApplicationArtifacts">
			    <WriteLinesToFile
			        File="{Escape(artifactsFile)}"
			        Lines="@(ApplicationArtifact->'%(FullPath)|%(ArtifactRole)|%(IsPrimary)|%(PrimaryArtifact)|%(PackageFormat)|%(PackageType)|%(PackageVersion)|%(Architecture)|%(RuntimeIdentifier)|%(Signed)|%(BundlePlatforms)|%(EntryPoint)|%(DeploymentDirectory)|%(ApplicationId)|%(ApplicationIdGuid)|%(ApplicationTitle)|%(ApplicationName)|%(ApplicationDisplayVersion)|%(ApplicationVersion)')"
			        Overwrite="true" />
			  </Target>
			</Project>
			""");

		Assert.True(
			DotnetInternal.Run("msbuild", $"\"{projectFile}\" -t:WriteApplicationArtifacts -v:minimal", output: _output),
			"Unable to execute the Windows ApplicationArtifact targets contract fixture.");

		var artifacts = File.ReadAllLines(artifactsFile)
			.Select(ArtifactRecord.Parse)
			.ToArray();

		Assert.Equal(13, artifacts.Length);
		Assert.Equal(artifacts.Length, artifacts.Select(artifact => artifact.Path).Distinct(StringComparer.OrdinalIgnoreCase).Count());
		Assert.DoesNotContain(artifacts, artifact => artifact.Path.StartsWith(intermediateDir, StringComparison.OrdinalIgnoreCase));
		Assert.DoesNotContain(artifacts, artifact => artifact.Path.EndsWith("loose-payload.dll", StringComparison.OrdinalIgnoreCase));

		AssertRole(primary, "Primary", isPrimary: true);
		AssertRole(packageTestDir, "PayloadDirectory");
		AssertRole(bundle, "Bundle");
		AssertRole(uploadPackage, "StoreUpload");
		AssertRole(storeUpload, "StoreUpload");
		AssertRole(appInstaller, "DeploymentManifest");
		AssertRole(symbols, "Symbols");
		AssertRole(certificate, "Certificate");
		AssertRole(installScript, "InstallScript");
		AssertRole(dependency, "DependencyPackage");
		AssertRole(landingPage, "LandingPage");
		AssertRole(support, "Support");
		AssertRole(appInstallerSupport, "Support");

		Assert.All(artifacts, artifact =>
		{
			Assert.Equal("MSIX", artifact.PackageType);
			Assert.Equal("2.3.4.5", artifact.PackageVersion);
			Assert.Equal("x64", artifact.Architecture);
			Assert.Equal("win-x64", artifact.RuntimeIdentifier);
			Assert.Equal("true", artifact.Signed, ignoreCase: true);
			Assert.Equal("extended-x64_arm64", artifact.BundlePlatforms);
			Assert.Equal("ArtifactApp.exe", artifact.EntryPoint);
			Assert.Equal(
				Path.TrimEndingDirectorySeparator(Path.GetFullPath(packageTestDir)),
				Path.TrimEndingDirectorySeparator(Path.GetFullPath(artifact.DeploymentDirectory)));
			Assert.Equal("11111111-2222-3333-4444-555555555555", artifact.ApplicationId);
			Assert.Equal(artifact.ApplicationId, artifact.ApplicationIdGuid);
			Assert.Equal("ms-resource:ArtifactApp", artifact.ApplicationTitle);
			Assert.Equal(artifact.ApplicationTitle, artifact.ApplicationName);
			Assert.Equal("2.3.4", artifact.ApplicationDisplayVersion);
			Assert.Equal("5", artifact.ApplicationVersion);
		});

		void AssertRole(string path, string role, bool isPrimary = false)
		{
			var artifact = Assert.Single(artifacts.Where(candidate =>
				string.Equals(
					Path.TrimEndingDirectorySeparator(Path.GetFullPath(candidate.Path)),
					Path.TrimEndingDirectorySeparator(Path.GetFullPath(path)),
					StringComparison.OrdinalIgnoreCase)));
			Assert.Equal(role, artifact.Role);
			Assert.Equal(isPrimary ? "true" : "false", artifact.IsPrimary, ignoreCase: true);
			Assert.Equal(
				Path.GetFullPath(isPrimary ? path : primary),
				Path.GetFullPath(artifact.PrimaryArtifact),
				ignoreCase: true);
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
		var targetsFile = Path.Combine(
			TestEnvironment.GetMauiDirectory(),
			"src",
			"Workload",
			PackageId,
			"buildTransitive",
			$"{PackageId}.targets");

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
			  </PropertyGroup>
			  <Target Name="Build" />
			  <Import Project="{Escape(targetsFile)}" />
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
	public void SparsePackageUsesTheFinalMsixAsPrimaryArtifact()
	{
		SetTestIdentifier(nameof(SparsePackageUsesTheFinalMsixAsPrimaryArtifact));
		var targetDir = Directory.CreateDirectory(Path.Combine(TestDirectory, "bin")).FullName;
		var sparsePackage = CreateFile(targetDir, "SparseApp.msix");
		var artifactsFile = Path.Combine(TestDirectory, "application-artifacts.txt");
		var projectFile = Path.Combine(TestDirectory, "Sparse.proj");
		var targetsFile = Path.Combine(
			TestEnvironment.GetMauiDirectory(),
			"src",
			"Workload",
			PackageId,
			"buildTransitive",
			$"{PackageId}.targets");

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
			  <Import Project="{Escape(targetsFile)}" />
			  <Target Name="WriteApplicationArtifacts" DependsOnTargets="GetApplicationArtifacts">
			    <WriteLinesToFile File="{Escape(artifactsFile)}" Lines="@(ApplicationArtifact->'%(FullPath)|%(ArtifactRole)|%(PackageType)|%(PackageFormat)')" Overwrite="true" />
			  </Target>
			</Project>
			""");

		Assert.True(
			DotnetInternal.Run("msbuild", $"\"{projectFile}\" -t:WriteApplicationArtifacts -v:minimal", output: _output),
			"Unable to execute the sparse Windows ApplicationArtifact fixture.");

		var fields = Assert.Single(File.ReadAllLines(artifactsFile)).Split('|');
		Assert.Equal(Path.GetFullPath(sparsePackage), Path.GetFullPath(fields[0]), ignoreCase: true);
		Assert.Equal("Primary", fields[1]);
		Assert.Equal("Sparse", fields[2]);
		Assert.Equal("msix", fields[3]);
	}

	[Fact]
	public void UnpackagedBuildAndPublishUseTheirOwnOutputPaths()
	{
		SetTestIdentifier(nameof(UnpackagedBuildAndPublishUseTheirOwnOutputPaths));
		var targetDir = Directory.CreateDirectory(Path.Combine(TestDirectory, "bin")).FullName;
		var publishDir = Directory.CreateDirectory(Path.Combine(targetDir, "publish")).FullName;
		var buildExecutable = CreateFile(targetDir, "UnpackagedApp.exe");
		var publishExecutable = CreateFile(publishDir, "UnpackagedApp.exe");
		var buildArtifactsFile = Path.Combine(TestDirectory, "build-application-artifacts.txt");
		var publishArtifactsFile = Path.Combine(TestDirectory, "publish-application-artifacts.txt");
		var projectFile = Path.Combine(TestDirectory, "Unpackaged.proj");
		var targetsFile = Path.Combine(
			TestEnvironment.GetMauiDirectory(),
			"src",
			"Workload",
			PackageId,
			"buildTransitive",
			$"{PackageId}.targets");

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
			    <ApplicationId>com.example.unpackaged</ApplicationId>
			    <ApplicationTitle>Unpackaged App</ApplicationTitle>
			    <ApplicationDisplayVersion>1.2.3</ApplicationDisplayVersion>
			    <ApplicationVersion>4</ApplicationVersion>
			  </PropertyGroup>
			  <Target Name="Build" />
			  <Target Name="PreparePublishOutput" />
			  <Import Project="{Escape(targetsFile)}" />
			  <Target Name="Publish" DependsOnTargets="PreparePublishOutput" />
			  <Target Name="WriteBuildApplicationArtifacts" DependsOnTargets="GetApplicationArtifacts">
			    <WriteLinesToFile File="{Escape(buildArtifactsFile)}" Lines="@(ApplicationArtifact->'%(FullPath)|%(ArtifactRole)')" Overwrite="true" />
			  </Target>
			  <Target Name="WritePublishedApplicationArtifacts" AfterTargets="Publish">
			    <WriteLinesToFile File="{Escape(publishArtifactsFile)}" Lines="@(ApplicationArtifact->'%(FullPath)|%(ArtifactRole)')" Overwrite="true" />
			  </Target>
			</Project>
			""");

		Assert.True(
			DotnetInternal.Run("msbuild", $"\"{projectFile}\" -t:WriteBuildApplicationArtifacts;Publish -v:minimal", output: _output),
			"Unable to collect unpackaged build and publish artifacts in one MSBuild invocation.");
		var sameInvocationPublishPrimary = Assert.Single(File.ReadAllLines(publishArtifactsFile).Where(line => line.EndsWith("|Primary", StringComparison.Ordinal)));
		Assert.Equal(Path.GetFullPath(publishExecutable), Path.GetFullPath(sameInvocationPublishPrimary.Split('|')[0]), ignoreCase: true);

		File.Delete(buildArtifactsFile);
		File.Delete(publishArtifactsFile);

		Assert.True(
			DotnetInternal.Run("msbuild", $"\"{projectFile}\" -t:WriteBuildApplicationArtifacts -v:minimal", output: _output),
			"Unable to collect unpackaged build artifacts.");
		var buildPrimary = Assert.Single(File.ReadAllLines(buildArtifactsFile).Where(line => line.EndsWith("|Primary", StringComparison.Ordinal)));
		Assert.Equal(Path.GetFullPath(buildExecutable), Path.GetFullPath(buildPrimary.Split('|')[0]), ignoreCase: true);

		Assert.True(
			DotnetInternal.Run("msbuild", $"\"{projectFile}\" -t:Publish -v:minimal", output: _output),
			"Unable to collect unpackaged publish artifacts.");
		var publishPrimary = Assert.Single(File.ReadAllLines(publishArtifactsFile).Where(line => line.EndsWith("|Primary", StringComparison.Ordinal)));
		Assert.Equal(Path.GetFullPath(publishExecutable), Path.GetFullPath(publishPrimary.Split('|')[0]), ignoreCase: true);
	}

	string FindPackage(string packageId)
	{
		var artifactDir = Path.Combine(TestEnvironment.GetMauiDirectory(), "artifacts");
		var matchingPackages = Directory
			.GetFiles(artifactDir, $"{packageId}.{MauiPackageVersion}.nupkg", SearchOption.AllDirectories)
			.Where(path => !path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase))
			.ToArray();

		return Assert.Single(matchingPackages);
	}

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
		string Role,
		string IsPrimary,
		string PrimaryArtifact,
		string PackageFormat,
		string PackageType,
		string PackageVersion,
		string Architecture,
		string RuntimeIdentifier,
		string Signed,
		string BundlePlatforms,
		string EntryPoint,
		string DeploymentDirectory,
		string ApplicationId,
		string ApplicationIdGuid,
		string ApplicationTitle,
		string ApplicationName,
		string ApplicationDisplayVersion,
		string ApplicationVersion)
	{
		public static ArtifactRecord Parse(string line)
		{
			var fields = line.Split('|');
			Assert.Equal(19, fields.Length);
			return new(
				fields[0],
				fields[1],
				fields[2],
				fields[3],
				fields[4],
				fields[5],
				fields[6],
				fields[7],
				fields[8],
				fields[9],
				fields[10],
				fields[11],
				fields[12],
				fields[13],
				fields[14],
				fields[15],
				fields[16],
				fields[17],
				fields[18]);
		}
	}
}
