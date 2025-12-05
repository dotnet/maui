namespace Microsoft.Maui.IntegrationTests;

[Category(Categories.WindowsTemplates)]
public class WindowsTemplateTest : BaseTemplateTests
{
	[Test]
	[TestCase("maui", DotNetPrevious, "Debug")]
	[TestCase("maui", DotNetPrevious, "Release")]
	[TestCase("maui", DotNetCurrent, "Debug")]
	[TestCase("maui", DotNetCurrent, "Release")]
	[TestCase("maui-blazor", DotNetPrevious, "Debug")]
	[TestCase("maui-blazor", DotNetPrevious, "Release")]
	[TestCase("maui-blazor", DotNetCurrent, "Debug")]
	[TestCase("maui-blazor", DotNetCurrent, "Release")]
	public void BuildPackaged(string id, string framework, string config)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
			$"Unable to create template {id}. Check test output for errors.");

		// .NET 9 and later was Unpackaged, so we need to remove the line
		FileUtilities.ReplaceInFile(projectFile,
			"<WindowsPackageType>None</WindowsPackageType>",
			"");

		Assert.IsTrue(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Test]
	[TestCase("maui", true, true, "None")]
	[TestCase("maui", true, true, "MSIX")]
	[TestCase("maui", true, false, "None")]
	[TestCase("maui", true, false, "MSIX")]
	[TestCase("maui", false, true, "None")]
	[TestCase("maui", false, true, "MSIX")]
	[TestCase("maui", false, false, "None")]
	[TestCase("maui", false, false, "MSIX")]
	public void BuildWindowsAppSDKSelfContained(string id, bool wasdkself, bool netself, string packageType)
	{
		if (TestEnvironment.IsMacOS)
		{
			Assert.Ignore("This test is designed for testing a windows build.");
		}

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, DotNetCurrent),
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

		Assert.IsTrue(DotnetInternal.Build(projectFile, "Debug", properties: extendedBuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Test]
	[TestCase("maui", true, "None")]
	[TestCase("maui", true, "MSIX")]
	[TestCase("maui", false, "None")]
	[TestCase("maui", false, "MSIX")]
	public void BuildWindowsRidGraph(string id, bool useRidGraph, string packageType)
	{
		if (TestEnvironment.IsMacOS)
		{
			Assert.Ignore("This test is designed for testing a windows build.");
		}

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, DotNetCurrent),
			$"Unable to create template {id}. Check test output for errors.");

		FileUtilities.ReplaceInFile(projectFile,
			"<WindowsPackageType>None</WindowsPackageType>",
			$"""
			<UseRidGraph>{useRidGraph}</UseRidGraph>
			<WindowsPackageType>{packageType}</WindowsPackageType>
			""");

		var extendedBuildProps = BuildProps;
		extendedBuildProps.Add($"TargetFramework={DotNetCurrent}-windows10.0.19041.0");

		Assert.IsTrue(DotnetInternal.Build(projectFile, "Debug", properties: extendedBuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	// [Test]
	// [TestCase("maui", DotNetCurrent, "Release", false)]
	// [TestCase("maui", DotNetPrevious, "Release", true)]
	// [TestCase("maui-blazor", DotNetCurrent, "Release", false)]
	// [TestCase("maui-blazor", DotNetPrevious, "Release", true)]
	public void PublishUnpackaged(string id, string framework, string config, bool usesRidGraph)
	{
		if (!TestEnvironment.IsWindows)
		{
			Assert.Ignore("Running Windows templates is only supported on Windows.");
		}

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
			$"Unable to create template {id}. Check test output for errors.");

		// .NET 9 is Unpackaged by default, so we don't have to do anything
		FileUtilities.ShouldContainInFile(projectFile,
			"<WindowsPackageType>None</WindowsPackageType>");

		Assert.IsTrue(DotnetInternal.Publish(projectFile, config, framework: $"{framework}-windows10.0.19041.0", properties: BuildProps),
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
			Assert.IsTrue(File.Exists(fullpath),
				$"Unable to find expected asset: {fullpath}");
		}
	}

	// [Test]
	// [TestCase("maui", DotNetCurrent, "Release", false)]
	// [TestCase("maui", DotNetPrevious, "Release", true)]
	// [TestCase("maui-blazor", DotNetCurrent, "Release", false)]
	// [TestCase("maui-blazor", DotNetPrevious, "Release", true)]
	public void PublishPackaged(string id, string framework, string config, bool usesRidGraph)
	{
		if (!TestEnvironment.IsWindows)
		{
			Assert.Ignore("Running Windows templates is only supported on Windows.");
		}

		var projectDir = TestDirectory;
		var name = Path.GetFileName(projectDir);
		var projectFile = Path.Combine(projectDir, $"{name}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
			$"Unable to create template {id}. Check test output for errors.");

		// .NET 9 and later was Unpackaged, so we need to remove the line
		FileUtilities.ReplaceInFile(projectFile,
			"<WindowsPackageType>None</WindowsPackageType>",
			"");

		Assert.IsTrue(DotnetInternal.Publish(projectFile, config, framework: $"{framework}-windows10.0.19041.0", properties: BuildProps),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

		var rid = usesRidGraph ? "win10-x64" : "win-x64";
		var assetsRoot = Path.Combine(projectDir, $"bin/{config}/{framework}-windows10.0.19041.0/{rid}/AppPackages/{name}_1.0.0.1_Test");

		AssetExists($"{name}_1.0.0.1_x64.msix");

		void AssetExists(string filename)
		{
			var fullpath = Path.Combine(assetsRoot!, filename);
			Assert.IsTrue(File.Exists(fullpath),
				$"Unable to find expected asset: {fullpath}");
		}
	}
}
