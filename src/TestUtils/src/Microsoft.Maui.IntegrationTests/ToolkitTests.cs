using System.Text.RegularExpressions;
using Microsoft.Maui.IntegrationTests.Android;

namespace Microsoft.Maui.IntegrationTests;

/// <summary>
/// Integration tests that verify .NET MAUI builds successfully with popular third-party toolkit packages.
///
/// These tests help catch regressions where MAUI changes break compatibility with widely-used community packages
/// such as CommunityToolkit.Maui and Syncfusion.Maui.Toolkit. By building template projects with these packages,
/// we ensure that MAUI's build system, MSBuild tasks, and SDK remain compatible with the broader MAUI ecosystem.
/// </summary>
[Trait("Category", "Build")]
public class ToolkitTests : BaseTemplateTests
{
	public ToolkitTests(IntegrationTestFixture fixture, ITestOutputHelper output) : base(fixture, output) { }

	/// <summary>
	/// Tests that a .NET 9 MAUI app can build successfully with CommunityToolkit.Maui package installed.
	/// </summary>
	[Theory]
	[InlineData("maui", DotNetPrevious, "Debug")]
	[InlineData("maui", DotNetPrevious, "Release")]
	public void BuildWithCommunityToolkit(string id, string framework, string config)
	{
		SetTestIdentifier(id, framework, config, "CommunityToolkit");
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework, output: _output),
			$"Unable to create template {id}. Check test output for errors.");

		AddPackageReference(projectFile, "CommunityToolkit.Maui", GetPackageVersion("CommunityToolkitMauiPackageVersion"));
		RegisterCommunityToolkit(projectDir);

		Assert.True(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Project {Path.GetFileName(projectFile)} with CommunityToolkit.Maui failed to build. Check test output/attachments for errors.");
	}

	/// <summary>
	/// Tests that a MAUI app can build successfully with Syncfusion.Maui.Toolkit package installed.
	/// </summary>
	[Theory]
	[InlineData("maui", DotNetCurrent, "Debug")]
	[InlineData("maui", DotNetCurrent, "Release")]
	public void BuildWithSyncfusionToolkit(string id, string framework, string config)
	{
		SetTestIdentifier(id, framework, config, "SyncfusionToolkit");
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework, output: _output),
			$"Unable to create template {id}. Check test output for errors.");

		AddPackageReference(projectFile, "Syncfusion.Maui.Toolkit", GetPackageVersion("SyncfusionMauiToolkitPackageVersion"));
		RegisterSyncfusionToolkit(projectDir);

		Assert.True(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Project {Path.GetFileName(projectFile)} with Syncfusion.Maui.Toolkit failed to build. Check test output/attachments for errors.");
	}

	/// <summary>
	/// Tests that a .NET 9 MAUI app can build successfully with both CommunityToolkit.Maui and Syncfusion.Maui.Toolkit packages.
	/// </summary>
	[Theory]
	[InlineData("maui", DotNetPrevious, "Debug")]
	[InlineData("maui", DotNetPrevious, "Release")]
	public void BuildWithMultipleToolkits(string id, string framework, string config)
	{
		SetTestIdentifier(id, framework, config, "MultipleToolkits");
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework, output: _output),
			$"Unable to create template {id}. Check test output for errors.");

		AddPackageReference(projectFile, "CommunityToolkit.Maui", GetPackageVersion("CommunityToolkitMauiPackageVersion"));
		AddPackageReference(projectFile, "Syncfusion.Maui.Toolkit", GetPackageVersion("SyncfusionMauiToolkitPackageVersion"));
		RegisterCommunityToolkit(projectDir);
		RegisterSyncfusionToolkit(projectDir);

		Assert.True(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Project {Path.GetFileName(projectFile)} with multiple toolkits failed to build. Check test output/attachments for errors.");
	}

	private static void AddPackageReference(string projectFile, string packageId, string version)
	{
		FileUtilities.ReplaceInFile(projectFile,
			"</Project>",
			$"""
			  <ItemGroup>
			    <PackageReference Include="{packageId}" Version="{version}" />
			  </ItemGroup>
			</Project>
			""");
	}

	private static void RegisterCommunityToolkit(string projectDir)
	{
		var mauiProgramFile = Path.Combine(projectDir, "MauiProgram.cs");

		FileUtilities.ReplaceInFile(mauiProgramFile,
			"using Microsoft.Extensions.Logging;",
			"""
			using Microsoft.Extensions.Logging;
			using CommunityToolkit.Maui;
			""");

		FileUtilities.ReplaceInFile(mauiProgramFile,
			".UseMauiApp<App>()",
			"""
			.UseMauiApp<App>()
				.UseMauiCommunityToolkit()
			""");
	}

	private static void RegisterSyncfusionToolkit(string projectDir)
	{
		var mauiProgramFile = Path.Combine(projectDir, "MauiProgram.cs");

		FileUtilities.ReplaceInFile(mauiProgramFile,
			"using Microsoft.Extensions.Logging;",
			"""
			using Microsoft.Extensions.Logging;
			using Syncfusion.Maui.Toolkit.Hosting;
			""");

		FileUtilities.ReplaceInFile(mauiProgramFile,
			".UseMauiApp<App>()",
			"""
			.UseMauiApp<App>()
				.ConfigureSyncfusionToolkit()
			""");
	}

	private static string GetPackageVersion(string propertyName)
	{
		var versionsPropsPath = Path.Combine(TestEnvironment.GetMauiDirectory(), "eng", "Versions.props");
		var versionsProps = System.Xml.Linq.XDocument.Load(versionsPropsPath);
		var version = versionsProps.Descendants(propertyName).FirstOrDefault()?.Value;

		if (string.IsNullOrEmpty(version))
			throw new Exception($"Could not find {propertyName} in Versions.props");

		return version;
	}
}

[Collection("Android Emulator Tests")]
[Trait("Category", "RunOnAndroid")]
public class AndroidToolkitTests : BaseBuildTest
{
	private readonly AndroidEmulatorFixture _emulatorFixture;
	private string testPackage = "";

	public AndroidToolkitTests(IntegrationTestFixture fixture, ITestOutputHelper output, AndroidEmulatorFixture emulatorFixture)
		: base(fixture, output)
	{
		_emulatorFixture = emulatorFixture;
	}

	public override void Dispose()
	{
		if (!string.IsNullOrEmpty(testPackage))
			Adb.UninstallPackage(testPackage);

		base.Dispose();
	}

	[Fact]
	public void RunSyncfusionToolkitOnAndroid()
	{
		var id = "maui";
		var framework = DotNetCurrent;
		var config = "Debug";

		SetTestIdentifier(id, framework, config, "SyncfusionToolkitAndroid");
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework, additionalDotNetNewParams: "--no-restore", output: _output),
			$"Unable to create template {id}. Check test output for errors.");

		StripNonAndroidTfms(projectFile, framework);
		AddSyncfusionToolkitPackage(projectFile);
		RegisterSyncfusionToolkit(projectDir);
		AddSyncfusionToolkitControl(projectDir);
		AddInstrumentation(projectDir);

		var buildProps = BuildProps;
		buildProps.Add($"TargetFrameworks={framework}-android");

		Assert.True(DotnetInternal.Build(projectFile, config, target: "Install", framework: $"{framework}-android", properties: buildProps, output: _output),
			$"Project {Path.GetFileName(projectFile)} with Syncfusion.Maui.Toolkit failed to install. Check test output/attachments for errors.");

		var xhResultsDir = Path.Combine(TestEnvironment.GetLogDirectory(), "xh-results", Path.GetFileName(projectDir));
		Directory.CreateDirectory(xhResultsDir);

		testPackage = $"com.companyname.{Path.GetFileName(projectDir).ToLowerInvariant()}";
		Assert.True(XHarness.RunAndroid(testPackage, xhResultsDir, -1, output: _output),
			$"Project {Path.GetFileName(projectFile)} with Syncfusion.Maui.Toolkit failed to run. Check test output/attachments for errors.");
	}

	private static void AddSyncfusionToolkitPackage(string projectFile)
	{
		var version = GetPackageVersion("SyncfusionMauiToolkitPackageVersion");

		FileUtilities.ReplaceInFile(projectFile,
			"</Project>",
			$"""
			  <ItemGroup>
			    <PackageReference Include="Syncfusion.Maui.Toolkit" Version="{version}" />
			  </ItemGroup>
			</Project>
			""");
	}

	private static void RegisterSyncfusionToolkit(string projectDir)
	{
		var mauiProgramFile = Path.Combine(projectDir, "MauiProgram.cs");

		FileUtilities.ReplaceInFile(mauiProgramFile,
			"using Microsoft.Extensions.Logging;",
			"""
			using Microsoft.Extensions.Logging;
			using Syncfusion.Maui.Toolkit.Hosting;
			""");

		FileUtilities.ReplaceInFile(mauiProgramFile,
			".UseMauiApp<App>()",
			"""
			.UseMauiApp<App>()
				.ConfigureSyncfusionToolkit()
			""");
	}

	private static void AddSyncfusionToolkitControl(string projectDir)
	{
		var mainPageFile = Path.Combine(projectDir, "MainPage.xaml");

		FileUtilities.ReplaceInFile(mainPageFile,
			"xmlns:x=\"http://schemas.microsoft.com/winfx/2009/xaml\"",
			"""
			xmlns:x="http://schemas.microsoft.com/winfx/2009/xaml"
			xmlns:syncfusionButtons="clr-namespace:Syncfusion.Maui.Toolkit.Buttons;assembly=Syncfusion.Maui.Toolkit"
			""");

		FileUtilities.ReplaceInFile(mainPageFile,
			"</VerticalStackLayout>",
			"""
				<syncfusionButtons:SfButton
					Content="Syncfusion Toolkit"
					HorizontalOptions="Fill" />
			</VerticalStackLayout>
			""");
	}

	private static void AddInstrumentation(string projectDir)
	{
		var androidDir = Path.Combine(projectDir, "Platforms", "Android");
		var instDestination = Path.Combine(androidDir, "Instrumentation.cs");
		FileUtilities.CreateFileFromResource("TemplateLaunchInstrumentation.cs", instDestination);
		Assert.True(File.Exists(instDestination), "Failed to create Instrumentation.cs");
		FileUtilities.ReplaceInFile(instDestination, "namespace mauitemplate", $"namespace {Path.GetFileName(projectDir)}");

		FileUtilities.ReplaceInFile(Path.Combine(androidDir, "MainActivity.cs"),
			"MainLauncher = true",
			"MainLauncher = true, Name = \"com.microsoft.mauitemplate.MainActivity\"");
	}

	private static void StripNonAndroidTfms(string projectFile, string framework)
	{
		var content = File.ReadAllText(projectFile);
		var androidTfm = $"{framework}-android";

		content = Regex.Replace(content,
			@"\s*<TargetFrameworks\s+Condition=""[^""]*"">[^<]*</TargetFrameworks>",
			"");
		content = Regex.Replace(content,
			@"<TargetFrameworks>[^<]*</TargetFrameworks>",
			$"<TargetFrameworks>{androidTfm}</TargetFrameworks>");

		File.WriteAllText(projectFile, content);
	}

	private static string GetPackageVersion(string propertyName)
	{
		var versionsPropsPath = Path.Combine(TestEnvironment.GetMauiDirectory(), "eng", "Versions.props");
		var versionsProps = System.Xml.Linq.XDocument.Load(versionsPropsPath);
		var version = versionsProps.Descendants(propertyName).FirstOrDefault()?.Value;

		if (string.IsNullOrEmpty(version))
			throw new Exception($"Could not find {propertyName} in Versions.props");

		return version;
	}
}
