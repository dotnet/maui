using System.Security;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Xml.Linq;

namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "WindowsTemplates")]
public class WindowsTemplateTest : BaseTemplateTests
{
	public WindowsTemplateTest(IntegrationTestFixture fixture, ITestOutputHelper output) : base(fixture, output) { }

	[Theory]
	[InlineData("maui", DotNetPrevious, "Debug")]
	[InlineData("maui", DotNetPrevious, "Release")]
	[InlineData("maui", DotNetCurrent, "Debug")]
	[InlineData("maui", DotNetCurrent, "Release")]
	[InlineData("maui-blazor", DotNetPrevious, "Debug")]
	[InlineData("maui-blazor", DotNetPrevious, "Release")]
	[InlineData("maui-blazor", DotNetCurrent, "Debug")]
	[InlineData("maui-blazor", DotNetCurrent, "Release")]
	public void BuildPackaged(string id, string framework, string config)
	{
		SetTestIdentifier(id, framework, config);
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework, output: _output),
			$"Unable to create template {id}. Check test output for errors.");

		// .NET 9 and later was Unpackaged, so we need to remove the line
		FileUtilities.ReplaceInFile(projectFile,
			"<WindowsPackageType>None</WindowsPackageType>",
			"");

		Assert.True(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Theory]
	[InlineData("maui", true, true, "None")]
	[InlineData("maui", true, true, "MSIX")]
	[InlineData("maui", true, false, "None")]
	[InlineData("maui", true, false, "MSIX")]
	[InlineData("maui", false, true, "None")]
	[InlineData("maui", false, true, "MSIX")]
	[InlineData("maui", false, false, "None")]
	[InlineData("maui", false, false, "MSIX")]
	public void BuildWindowsAppSDKSelfContained(string id, bool wasdkself, bool netself, string packageType)
	{
		SetTestIdentifier(id, wasdkself, netself, packageType);
		if (TestEnvironment.IsMacOS)
		{
			if (true)
				return; // Skip: "This test is designed for testing a windows build."
		}

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, DotNetCurrent, output: _output),
			$"Unable to create template {id}. Check test output for errors.");

		FileUtilities.ReplaceInFile(projectFile,
			"<WindowsPackageType>None</WindowsPackageType>",
			$"""
			<WindowsAppSDKSelfContained>{wasdkself}</WindowsAppSDKSelfContained>
			<SelfContained>{netself}</SelfContained>
			<WindowsPackageType>{packageType}</WindowsPackageType>
			""");

		var extendedBuildProps = BuildProps;
		extendedBuildProps.Add($"TargetFramework={DotNetCurrent}-windows10.0.19041.0");

		Assert.True(DotnetInternal.Build(projectFile, "Debug", properties: extendedBuildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Theory]
	[InlineData("maui", true, "None")]
	[InlineData("maui", true, "MSIX")]
	[InlineData("maui", false, "None")]
	[InlineData("maui", false, "MSIX")]
	public void BuildWindowsRidGraph(string id, bool useRidGraph, string packageType)
	{
		SetTestIdentifier(id, useRidGraph, packageType);
		if (TestEnvironment.IsMacOS)
		{
			if (true)
				return; // Skip: "This test is designed for testing a windows build."
		}

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, DotNetCurrent, output: _output),
			$"Unable to create template {id}. Check test output for errors.");

		FileUtilities.ReplaceInFile(projectFile,
			"<WindowsPackageType>None</WindowsPackageType>",
			$"""
			<UseRidGraph>{useRidGraph}</UseRidGraph>
			<WindowsPackageType>{packageType}</WindowsPackageType>
			""");

		var extendedBuildProps = BuildProps;
		extendedBuildProps.Add($"TargetFramework={DotNetCurrent}-windows10.0.19041.0");

		Assert.True(DotnetInternal.Build(projectFile, "Debug", properties: extendedBuildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Theory]
	[InlineData("maui", DotNetCurrent, "Release", false)]
	//[InlineData("maui", DotNetPrevious, "Release", true)]
	[InlineData("maui-blazor", DotNetCurrent, "Release", false)]
	//[InlineData("maui-blazor", DotNetPrevious, "Release", true)]
	public void PublishUnpackaged(string id, string framework, string config, bool usesRidGraph)
	{
		SetTestIdentifier(id, framework, config, usesRidGraph);
		if (!TestEnvironment.IsWindows)
		{
			if (true)
				return; // Skip: "Running Windows templates is only supported on Windows."
		}

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework, output: _output),
			$"Unable to create template {id}. Check test output for errors.");

		// .NET 9 is Unpackaged by default, so we don't have to do anything
		FileUtilities.ShouldContainInFile(projectFile,
			"<WindowsPackageType>None</WindowsPackageType>");

		Assert.True(DotnetInternal.Publish(projectFile, config, framework: $"{framework}-windows10.0.19041.0", properties: BuildProps, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

		var rid = usesRidGraph ? "win10-x64" : "win-x64";
		var assetsRoot = Path.Combine(projectDir, $"bin/{config}/{framework}-windows10.0.19041.0/{rid}/publish");

		AssetExists("dotnet_bot.scale-100.png");
		AssetExists("appiconLogo.scale-100.png");
		AssetExists("OpenSans-Regular.ttf");
		AssetExists("splashSplashScreen.scale-100.png");
		AssetExists("AboutAssets.txt");

		void AssetExists(string filename)
		{
			var fullpath = Path.Combine(assetsRoot!, filename);
			Assert.True(File.Exists(fullpath),
				$"Unable to find expected asset: {fullpath}");
		}
	}

	[Theory]
	[InlineData("maui", DotNetCurrent, "Release", false)]
	//[InlineData("maui", DotNetPrevious, "Release", true)]
	[InlineData("maui-blazor", DotNetCurrent, "Release", false)]
	//[InlineData("maui-blazor", DotNetPrevious, "Release", true)]
	public void PublishPackaged(string id, string framework, string config, bool usesRidGraph)
	{
		SetTestIdentifier(id, framework, config, usesRidGraph);
		if (!TestEnvironment.IsWindows)
		{
			if (true)
				return; // Skip: "Running Windows templates is only supported on Windows."
		}

		var projectDir = TestDirectory;
		var name = Path.GetFileName(projectDir);
		var projectFile = Path.Combine(projectDir, $"{name}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework, output: _output),
			$"Unable to create template {id}. Check test output for errors.");

		// .NET 9 and later was Unpackaged, so we need to remove the line
		FileUtilities.ReplaceInFile(projectFile,
			"<WindowsPackageType>None</WindowsPackageType>",
			"");

		Assert.True(DotnetInternal.Publish(projectFile, config, framework: $"{framework}-windows10.0.19041.0", properties: BuildProps, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

		var rid = usesRidGraph ? "win10-x64/" : "";
		var prefix = framework == DotNetCurrent
			? ""
			: $"bin/{config}/{framework}-windows10.0.19041.0/";
		var assetsRoot = Path.Combine(projectDir, $"{prefix}{rid}AppPackages/{name}_1.0.0.1_Test");

		AssetExists($"{name}_1.0.0.1_x64.msix");

		void AssetExists(string filename)
		{
			var fullpath = Path.Combine(assetsRoot!, filename);
			Assert.True(File.Exists(fullpath),
				$"Unable to find expected asset: {fullpath}");
		}
	}

	[Fact]
	public void BuildWithIdentityClient()
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New("maui", projectDir, DotNetCurrent, output: _output),
			$"Unable to create template maui. Check test output for errors.");

		// .NET 9 and later was Unpackaged, so we need to remove the line
		FileUtilities.ReplaceInFile(projectFile,
			"</Project>",
			"""
			<ItemGroup Condition="$(TargetFramework.Contains('-windows'))">
			  <PackageReference Include="Microsoft.Identity.Client" Version="4.79.1" />
			  <PackageReference Include="Microsoft.Identity.Client.Desktop.WinUI3" Version="4.79.1"/>
			</ItemGroup>
			</Project>
			""");

		Assert.True(DotnetInternal.Build(projectFile, "Debug", properties: BuildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Theory]
	[InlineData("None")]
	[InlineData("MSIX")]
	public void ApplicationArtifactsAreAvailableForMauiBuildAndPublish(string packageType)
	{
		SetTestIdentifier(packageType);
		if (!TestEnvironment.IsWindows)
		{
			if (true)
				return; // Skip: "Running Windows templates is only supported on Windows."
		}

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");
		var buildArtifactsFile = Path.Combine(projectDir, "build-application-artifacts.txt");
		var publishArtifactsFile = Path.Combine(projectDir, "publish-application-artifacts.txt");

		Assert.True(DotnetInternal.New("maui", projectDir, DotNetCurrent, output: _output),
			"Unable to create the MAUI template. Check test output for errors.");

		var signingProperties = string.Empty;
		if (packageType == "MSIX")
		{
			const string certificatePassword = "application-artifacts-test";
			var certificatePath = Path.Combine(projectDir, "ApplicationArtifactsTest.pfx");
			CreateSigningCertificate(certificatePath, certificatePassword);
			signingProperties = $"""
				    <GenerateAppxPackageOnBuild>true</GenerateAppxPackageOnBuild>
				    <PublishAppxPackage>true</PublishAppxPackage>
				    <AppxPackageSigningEnabled>true</AppxPackageSigningEnabled>
				    <PackageCertificateKeyFile>{Escape(certificatePath)}</PackageCertificateKeyFile>
				    <PackageCertificatePassword>{certificatePassword}</PackageCertificatePassword>
				""";
		}

		FileUtilities.ReplaceInFile(
			projectFile,
			"<WindowsPackageType>None</WindowsPackageType>",
			$"""
			<WindowsPackageType>{packageType}</WindowsPackageType>
			{signingProperties}
			""");
		AddApplicationArtifactWriters(projectFile, buildArtifactsFile, publishArtifactsFile);

		var framework = $"{DotNetCurrent}-windows10.0.19041.0";
		Assert.True(
			DotnetInternal.Build(projectFile, "Debug", target: "WriteBuildApplicationArtifacts", framework: framework, properties: BuildProps, output: _output),
			"The MAUI project failed to collect build ApplicationArtifact items.");
		AssertApplicationArtifacts(File.ReadAllLines(buildArtifactsFile), packageType, published: false);

		Assert.True(
			DotnetInternal.Publish(projectFile, "Debug", framework: framework, properties: BuildProps, output: _output),
			"The MAUI project failed to publish.");
		AssertApplicationArtifacts(File.ReadAllLines(publishArtifactsFile), packageType, published: true);
	}

	[Fact]
	public void ApplicationArtifactsIncludeBundleStoreUploadAndAppInstaller()
	{
		SetTestIdentifier(nameof(ApplicationArtifactsIncludeBundleStoreUploadAndAppInstaller));
		if (!TestEnvironment.IsWindows)
		{
			if (true)
				return; // Skip: "Running Windows templates is only supported on Windows."
		}

		const string certificatePassword = "application-artifacts-test";
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");
		var certificatePath = Path.Combine(projectDir, "ApplicationArtifactsTest.pfx");
		var publishArtifactsFile = Path.Combine(projectDir, "publish-application-artifacts.txt");

		Assert.True(DotnetInternal.New("maui", projectDir, DotNetCurrent, output: _output),
			"Unable to create the MAUI template. Check test output for errors.");

		CreateSigningCertificate(certificatePath, certificatePassword);
		FileUtilities.ReplaceInFile(
			projectFile,
			"<WindowsPackageType>None</WindowsPackageType>",
			$"""
			<WindowsPackageType>MSIX</WindowsPackageType>
			<GenerateAppxPackageOnBuild>true</GenerateAppxPackageOnBuild>
			<PublishAppxPackage>true</PublishAppxPackage>
			<AppxPackageSigningEnabled>true</AppxPackageSigningEnabled>
			<PackageCertificateKeyFile>{Escape(certificatePath)}</PackageCertificateKeyFile>
			<PackageCertificatePassword>{certificatePassword}</PackageCertificatePassword>
			<UapAppxPackageBuildMode>StoreAndSideload</UapAppxPackageBuildMode>
			<AppxBundle>Always</AppxBundle>
			<AppxBundlePlatforms>x64</AppxBundlePlatforms>
			<GenerateAppInstallerFile>true</GenerateAppInstallerFile>
			<AppInstallerUri>https://example.invalid/application-artifacts/</AppInstallerUri>
			<HoursBetweenUpdateChecks>0</HoursBetweenUpdateChecks>
			""");
		AddApplicationArtifactWriters(projectFile, Path.Combine(projectDir, "build-application-artifacts.txt"), publishArtifactsFile);

		var framework = $"{DotNetCurrent}-windows10.0.19041.0";
		Assert.True(
			DotnetInternal.Publish(projectFile, "Release", framework: framework, properties: BuildProps, output: _output),
			"The MAUI project failed to produce the Windows packaging output matrix.");

		var artifactLines = File.ReadAllLines(publishArtifactsFile);
		var artifacts = artifactLines.Select(WindowsArtifact.Parse).ToArray();
		AssertApplicationArtifacts(artifactLines, "MSIX", published: true);
		Assert.Contains(artifacts, artifact => artifact.Role == "Bundle" && artifact.Path.EndsWith(".msixbundle", StringComparison.OrdinalIgnoreCase));
		Assert.Contains(artifacts, artifact => artifact.Role == "StoreUpload" && artifact.Path.EndsWith(".msixupload", StringComparison.OrdinalIgnoreCase));
		Assert.Contains(artifacts, artifact => artifact.Role == "DeploymentManifest" && artifact.Path.EndsWith(".appinstaller", StringComparison.OrdinalIgnoreCase));
	}

	[Fact]
	public void StandaloneWindowsAppSdkApplicationCanReferenceApplicationArtifactsPackage()
	{
		SetTestIdentifier(nameof(StandaloneWindowsAppSdkApplicationCanReferenceApplicationArtifactsPackage));
		if (!TestEnvironment.IsWindows)
		{
			if (true)
				return; // Skip: "Running Windows templates is only supported on Windows."
		}

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, "StandaloneWindowsApp.csproj");
		var buildArtifactsFile = Path.Combine(projectDir, "build-application-artifacts.txt");
		var publishArtifactsFile = Path.Combine(projectDir, "publish-application-artifacts.txt");
		var windowsAppSdkVersion = GetWindowsAppSdkPackageVersion();

		File.WriteAllText(Path.Combine(projectDir, "Program.cs"), """
			namespace StandaloneWindowsApp;

			internal static class Program
			{
				[STAThread]
				static void Main()
				{
				}
			}
			""");
		File.WriteAllText(projectFile, $"""
			<Project Sdk="Microsoft.NET.Sdk">
			  <PropertyGroup>
			    <TargetFramework>{DotNetCurrent}-windows10.0.19041.0</TargetFramework>
			    <OutputType>WinExe</OutputType>
			    <RuntimeIdentifier>win-x64</RuntimeIdentifier>
			    <WindowsPackageType>None</WindowsPackageType>
			    <ApplicationId>com.example.standalonewindowsapp</ApplicationId>
			    <ApplicationTitle>Standalone Windows App</ApplicationTitle>
			    <ApplicationDisplayVersion>7.8.9</ApplicationDisplayVersion>
			    <ApplicationVersion>10</ApplicationVersion>
			  </PropertyGroup>
			  <ItemGroup>
			    <PackageReference Include="Microsoft.WindowsAppSDK" Version="{windowsAppSdkVersion}" />
			    <PackageReference Include="Microsoft.Maui.ApplicationArtifacts.Windows" Version="{MauiPackageVersion}" />
			  </ItemGroup>
			  {GetApplicationArtifactWriters(buildArtifactsFile, publishArtifactsFile)}
			</Project>
			""");

		Assert.True(
			DotnetInternal.Build(projectFile, "Debug", target: "WriteBuildApplicationArtifacts", properties: BuildProps, output: _output),
			"The standalone Windows App SDK project failed to collect build ApplicationArtifact items.");
		AssertApplicationArtifacts(File.ReadAllLines(buildArtifactsFile), "None", published: false);

		Assert.True(
			DotnetInternal.Publish(projectFile, "Debug", properties: BuildProps, output: _output),
			"The standalone Windows App SDK project failed to publish.");
		AssertApplicationArtifacts(File.ReadAllLines(publishArtifactsFile), "None", published: true);

		var assetsFile = File.ReadAllText(Path.Combine(projectDir, "obj", "project.assets.json"));
		Assert.DoesNotContain("Microsoft.Maui.Core/", assetsFile, StringComparison.OrdinalIgnoreCase);
		Assert.DoesNotContain("Microsoft.Maui.Resizetizer/", assetsFile, StringComparison.OrdinalIgnoreCase);
	}

	static void AddApplicationArtifactWriters(string projectFile, string buildArtifactsFile, string publishArtifactsFile)
	{
		FileUtilities.ReplaceInFile(
			projectFile,
			"</Project>",
			$"""
			  {GetApplicationArtifactWriters(buildArtifactsFile, publishArtifactsFile)}
			</Project>
			""");
	}

	static string GetApplicationArtifactWriters(string buildArtifactsFile, string publishArtifactsFile) => $"""
		<Target Name="WriteBuildApplicationArtifacts" DependsOnTargets="GetApplicationArtifacts">
		  <WriteLinesToFile
		      File="{Escape(buildArtifactsFile)}"
		      Lines="@(ApplicationArtifact->'%(FullPath)|%(ArtifactRole)|%(PackageFormat)|%(PackageType)|%(PackageVersion)|%(Signed)|%(ApplicationId)|%(ApplicationTitle)|%(ApplicationDisplayVersion)|%(ApplicationVersion)')"
		      Overwrite="true" />
		</Target>
		<Target Name="WritePublishedApplicationArtifacts" AfterTargets="Publish">
		  <WriteLinesToFile
		      File="{Escape(publishArtifactsFile)}"
		      Lines="@(ApplicationArtifact->'%(FullPath)|%(ArtifactRole)|%(PackageFormat)|%(PackageType)|%(PackageVersion)|%(Signed)|%(ApplicationId)|%(ApplicationTitle)|%(ApplicationDisplayVersion)|%(ApplicationVersion)')"
		      Overwrite="true" />
		</Target>
		""";

	static void AssertApplicationArtifacts(string[] artifactLines, string packageType, bool published)
	{
		var artifacts = artifactLines.Select(WindowsArtifact.Parse).ToArray();
		Assert.NotEmpty(artifacts);
		Assert.Equal(artifacts.Length, artifacts.Select(artifact => artifact.Path).Distinct(StringComparer.OrdinalIgnoreCase).Count());
		Assert.DoesNotContain(artifacts, artifact =>
			artifact.Path.Contains($"{Path.DirectorySeparatorChar}obj{Path.DirectorySeparatorChar}", StringComparison.OrdinalIgnoreCase));
		Assert.All(artifacts, artifact =>
		{
			Assert.False(string.IsNullOrWhiteSpace(artifact.Role));
			Assert.False(string.IsNullOrWhiteSpace(artifact.Format));
			Assert.Equal(packageType, artifact.PackageType, ignoreCase: true);
			Assert.False(string.IsNullOrWhiteSpace(artifact.ApplicationId));
			Assert.False(string.IsNullOrWhiteSpace(artifact.ApplicationTitle));
			Assert.False(string.IsNullOrWhiteSpace(artifact.ApplicationDisplayVersion));
			Assert.False(string.IsNullOrWhiteSpace(artifact.ApplicationVersion));
		});

		var primary = Assert.Single(artifacts.Where(artifact => artifact.Role == "Primary"));
		Assert.Equal(packageType == "None" ? "exe" : "msix", primary.Format, ignoreCase: true);
		if (published && packageType == "None")
			Assert.Contains($"{Path.DirectorySeparatorChar}publish{Path.DirectorySeparatorChar}", primary.Path, StringComparison.OrdinalIgnoreCase);

		Assert.Contains(artifacts, artifact => artifact.Role == "PayloadDirectory");
		if (packageType == "MSIX")
		{
			Assert.Equal("true", primary.Signed, ignoreCase: true);
			Assert.False(string.IsNullOrWhiteSpace(primary.PackageVersion));
			Assert.Contains(artifacts, artifact => artifact.Role == "Certificate");
			Assert.Contains(artifacts, artifact => artifact.Role == "InstallScript");
		}
		else
		{
			Assert.Equal("false", primary.Signed, ignoreCase: true);
		}
	}

	static void CreateSigningCertificate(string path, string password)
	{
		using var rsa = RSA.Create(2048);
		var request = new CertificateRequest("CN=User Name", rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
		request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, false));
		request.CertificateExtensions.Add(new X509KeyUsageExtension(X509KeyUsageFlags.DigitalSignature, false));
		var enhancedKeyUsages = new OidCollection
		{
			new Oid("1.3.6.1.5.5.7.3.3")
		};
		request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(enhancedKeyUsages, false));
		using var certificate = request.CreateSelfSigned(DateTimeOffset.UtcNow.AddDays(-1), DateTimeOffset.UtcNow.AddDays(30));
		File.WriteAllBytes(path, certificate.Export(X509ContentType.Pfx, password));
	}

	static string GetWindowsAppSdkPackageVersion()
	{
		var versionsFile = Path.Combine(TestEnvironment.GetMauiDirectory(), "eng", "Versions.props");
		var versions = XDocument.Load(versionsFile);
		return versions.Descendants("MicrosoftWindowsAppSDKPackageVersion").Single().Value;
	}

	static string Escape(string value) => SecurityElement.Escape(value) ?? string.Empty;

	sealed record WindowsArtifact(
		string Path,
		string Role,
		string Format,
		string PackageType,
		string PackageVersion,
		string Signed,
		string ApplicationId,
		string ApplicationTitle,
		string ApplicationDisplayVersion,
		string ApplicationVersion)
	{
		public static WindowsArtifact Parse(string line)
		{
			var fields = line.Split('|');
			Assert.Equal(10, fields.Length);
			return new(fields[0], fields[1], fields[2], fields[3], fields[4], fields[5], fields[6], fields[7], fields[8], fields[9]);
		}
	}
}
