namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "WindowsTemplates")]
public class WindowsTemplateTest : BaseTemplateTests
{
	public WindowsTemplateTest(IntegrationTestFixture fixture, ITestOutputHelper output) : base(fixture, output) { }

	[Theory]
	// TODO: Re-enable net9.0 tests - see https://github.com/dotnet/maui/issues/XXXXX
	// net9.0 tests use Xcode 26.0, net10.0 uses Xcode 26.2 - can't have both on same machine
	// [InlineData("maui", DotNetPrevious, "Debug")]
	// [InlineData("maui", DotNetPrevious, "Release")]
	[InlineData("maui", DotNetCurrent, "Debug")]
	[InlineData("maui", DotNetCurrent, "Release")]
	// [InlineData("maui-blazor", DotNetPrevious, "Debug")]
	// [InlineData("maui-blazor", DotNetPrevious, "Release")]
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
			if (true) return; // Skip: "This test is designed for testing a windows build."
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
			if (true) return; // Skip: "This test is designed for testing a windows build."
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
	// TODO: Re-enable net9.0 tests - see https://github.com/dotnet/maui/issues/XXXXX
	// [InlineData("maui", DotNetPrevious, "Release", true)]
	[InlineData("maui-blazor", DotNetCurrent, "Release", false)]
	// [InlineData("maui-blazor", DotNetPrevious, "Release", true)]
	public void PublishUnpackaged(string id, string framework, string config, bool usesRidGraph)
	{
		SetTestIdentifier(id, framework, config, usesRidGraph);
		if (!TestEnvironment.IsWindows)
		{
			if (true) return; // Skip: "Running Windows templates is only supported on Windows."
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
	// TODO: Re-enable net9.0 tests - see https://github.com/dotnet/maui/issues/XXXXX
	// [InlineData("maui", DotNetPrevious, "Release", true)]
	[InlineData("maui-blazor", DotNetCurrent, "Release", false)]
	// [InlineData("maui-blazor", DotNetPrevious, "Release", true)]
	public void PublishPackaged(string id, string framework, string config, bool usesRidGraph)
	{
		SetTestIdentifier(id, framework, config, usesRidGraph);
		if (!TestEnvironment.IsWindows)
		{
			if (true) return; // Skip: "Running Windows templates is only supported on Windows."
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
}
