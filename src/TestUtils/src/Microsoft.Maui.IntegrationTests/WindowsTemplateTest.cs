namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", Categories.WindowsTemplates)]
public class WindowsTemplateTest : BaseTemplateTests
{	[Theory]
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
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework),
			$"Unable to create template {id}. Check test output for errors.");

		// TODO: remove this if as we should be able to build tizen net8
		if (framework != DotNetPrevious)
		{
			EnableTizen(projectFile);
		}

		if (framework == DotNetPrevious)
		{
			// .NET 8 was Packaged by default, so we don't have to do anything
			FileUtilities.ShouldNotContainInFile(projectFile,
				"<WindowsPackageType>");
		}
		else
		{
			// .NET 9 and later was Unpackaged, so we need to remove the line
			FileUtilities.ReplaceInFile(projectFile,
				"<WindowsPackageType>None</WindowsPackageType>",
				"");
		}

		Assert.True(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}	[Theory]
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
		if (TestEnvironment.IsMacOS)
		{
			Assert.Ignore("This test is designed for testing a windows build.");
		}

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, DotNetCurrent),
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

		Assert.True(DotnetInternal.Build(projectFile, "Debug", properties: extendedBuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}	[Theory]
[InlineData("maui", true, "None")]
[InlineData("maui", true, "MSIX")]
[InlineData("maui", false, "None")]
[InlineData("maui", false, "MSIX")]

	public void BuildWindowsRidGraph(string id, bool useRidGraph, string packageType)
	{
		if (TestEnvironment.IsMacOS)
		{
			Assert.Ignore("This test is designed for testing a windows build.");
		}

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, DotNetCurrent),
			$"Unable to create template {id}. Check test output for errors.");

		FileUtilities.ReplaceInFile(projectFile,
			"<WindowsPackageType>None</WindowsPackageType>",
			$"""
			<UseRidGraph>{useRidGraph}</UseRidGraph>
			<WindowsPackageType>{packageType}</WindowsPackageType>
			""");

		var extendedBuildProps = BuildProps;
		extendedBuildProps.Add($"TargetFramework={DotNetCurrent}-windows10.0.19041.0");

		Assert.True(DotnetInternal.Build(projectFile, "Debug", properties: extendedBuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}	[Theory]
[InlineData("maui", DotNetCurrent, "Release")]
[InlineData("maui", DotNetPrevious, "Release")]
[InlineData("maui-blazor", DotNetCurrent, "Release")]
[InlineData("maui-blazor", DotNetPrevious, "Release")]

	public void PublishUnpackaged(string id, string framework, string config)
	{
		if (!TestEnvironment.IsWindows)
		{
			Assert.Ignore("Running Windows templates is only supported on Windows.");
		}

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework),
			$"Unable to create template {id}. Check test output for errors.");

		if (framework == DotNetPrevious)
		{
			// .NET 8 was Packaged by default, so we need to say no
			FileUtilities.ShouldNotContainInFile(projectFile,
				"<WindowsPackageType>");
			BuildProps.Add("WindowsPackageType=None");
		}
		else
		{
			// .NET 9 is Unpackaged by default, so we don't have to do anything
			FileUtilities.ShouldContainInFile(projectFile,
				"<WindowsPackageType>None</WindowsPackageType>");
		}

		Assert.True(DotnetInternal.Publish(projectFile, config, framework: $"{framework}-windows10.0.19041.0", properties: BuildProps),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

		var assetsRoot = Path.Combine(projectDir, $"bin/{config}/{framework}-windows10.0.19041.0/win10-x64/publish");

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
	}	[Theory]
[InlineData("maui", DotNetCurrent, "Release")]
[InlineData("maui", DotNetPrevious, "Release")]
[InlineData("maui-blazor", DotNetCurrent, "Release")]
[InlineData("maui-blazor", DotNetPrevious, "Release")]

	public void PublishPackaged(string id, string framework, string config)
	{
		if (!TestEnvironment.IsWindows)
		{
			Assert.Ignore("Running Windows templates is only supported on Windows.");
		}

		var projectDir = TestDirectory;
		var name = Path.GetFileName(projectDir);
		var projectFile = Path.Combine(projectDir, $"{name}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework),
			$"Unable to create template {id}. Check test output for errors.");

		if (framework == DotNetPrevious)
		{
			// .NET 8 was Packaged by default, so we don't have to do anything
			FileUtilities.ShouldNotContainInFile(projectFile,
				"<WindowsPackageType>");
		}
		else
		{
			// .NET 9 and later was Unpackaged, so we need to remove the line
			FileUtilities.ReplaceInFile(projectFile,
				"<WindowsPackageType>None</WindowsPackageType>",
				"");
		}

		Assert.True(DotnetInternal.Publish(projectFile, config, framework: $"{framework}-windows10.0.19041.0", properties: BuildProps),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

		var assetsRoot = Path.Combine(projectDir, $"bin/{config}/{framework}-windows10.0.19041.0/win10-x64/AppPackages/{name}_1.0.0.1_Test");

		AssetExists($"{name}_1.0.0.1_x64.msix");

		void AssetExists(string filename)
		{
			var fullpath = Path.Combine(assetsRoot!, filename);
			Assert.True(File.Exists(fullpath),
				$"Unable to find expected asset: {fullpath}");
		}
	}
}
