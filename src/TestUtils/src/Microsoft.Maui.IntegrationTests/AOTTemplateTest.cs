namespace Microsoft.Maui.IntegrationTests;

[Category(Categories.AOT)]
public class AOTTemplateTest : BaseTemplateTests
{
	[Test]
	[TestCase("maui", $"{DotNetCurrent}-ios", "ios-arm64")]
	[TestCase("maui", $"{DotNetCurrent}-ios", "iossimulator-arm64")]
	[TestCase("maui", $"{DotNetCurrent}-ios", "iossimulator-x64")]
	[TestCase("maui", $"{DotNetCurrent}-maccatalyst", "maccatalyst-arm64")]
	[TestCase("maui", $"{DotNetCurrent}-maccatalyst", "maccatalyst-x64")]
	[TestCase("maui", $"{DotNetCurrent}-windows10.0.19041.0", "win-x64")]
	[TestCase("maui", $"{DotNetCurrent}-windows10.0.19041.0", "win-arm64")]
	public void PublishNativeAOT(string id, string framework, string runtimeIdentifier)
	{
		bool isWindowsFramework = framework.Contains("windows", StringComparison.OrdinalIgnoreCase);
		bool isApplePlatform = framework.Contains("ios", StringComparison.OrdinalIgnoreCase) || framework.Contains("maccatalyst", StringComparison.OrdinalIgnoreCase);

		if (isApplePlatform && !TestEnvironment.IsMacOS)
			Assert.Ignore("Publishing a MAUI iOS/macOS app with NativeAOT is only supported on a host MacOS system.");

		if (isWindowsFramework && !TestEnvironment.IsWindows)
			Assert.Ignore("Publishing a MAUI Windows app with NativeAOT is only supported on a host Windows system.");

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, DotNetCurrent),
			$"Unable to create template {id}. Check test output for errors.");

		var extendedBuildProps = isWindowsFramework ? PrepareNativeAotBuildPropsWindows(runtimeIdentifier) : PrepareNativeAotBuildProps();

		string binLogFilePath = $"publish-{DateTime.UtcNow.ToFileTimeUtc()}.binlog";
		Assert.IsTrue(DotnetInternal.Build(projectFile, "Release", framework: framework, properties: extendedBuildProps, runtimeIdentifier: runtimeIdentifier, binlogPath: binLogFilePath),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

		var actualWarnings = BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog(binLogFilePath);
		actualWarnings.AssertNoWarnings();
	}

	[Test]
	[TestCase("maui", $"{DotNetCurrent}-ios", "ios-arm64")]
	[TestCase("maui", $"{DotNetCurrent}-ios", "iossimulator-arm64")]
	[TestCase("maui", $"{DotNetCurrent}-ios", "iossimulator-x64")]
	[TestCase("maui", $"{DotNetCurrent}-maccatalyst", "maccatalyst-arm64")]
	[TestCase("maui", $"{DotNetCurrent}-maccatalyst", "maccatalyst-x64")]
	[TestCase("maui", $"{DotNetCurrent}-windows10.0.19041.0", "win-x64")]
	[TestCase("maui", $"{DotNetCurrent}-windows10.0.19041.0", "win-arm64")]
	public void PublishNativeAOTRootAllMauiAssemblies(string id, string framework, string runtimeIdentifier)
	{
		// This test follows the following guide: https://devblogs.microsoft.com/dotnet/creating-aot-compatible-libraries/#publishing-a-test-application-for-aot
		bool isWindowsFramework = framework.Contains("windows", StringComparison.OrdinalIgnoreCase);
		bool isApplePlatform = framework.Contains("ios", StringComparison.OrdinalIgnoreCase) || framework.Contains("maccatalyst", StringComparison.OrdinalIgnoreCase);

		if (isApplePlatform && !TestEnvironment.IsMacOS)
			Assert.Ignore("Publishing a MAUI iOS/macOS app with NativeAOT is only supported on a host MacOS system.");

		if (isWindowsFramework && !TestEnvironment.IsWindows)
			Assert.Ignore("Publishing a MAUI Windows app with NativeAOT is only supported on a host Windows system.");

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, DotNetCurrent),
			$"Unable to create template {id}. Check test output for errors.");

		var extendedBuildProps = isWindowsFramework ? PrepareNativeAotBuildPropsWindows(runtimeIdentifier) : PrepareNativeAotBuildProps();
		FileUtilities.ReplaceInFile(projectFile,
			"</Project>",
			"""
				<ItemGroup>
					<PackageReference Include="Microsoft.Maui.Controls.Foldable" Version="$(MauiVersion)" />
					<PackageReference Include="Microsoft.Maui.Controls.Maps" Version="$(MauiVersion)" />
					<PackageReference Include="Microsoft.Maui.Graphics.Skia" Version="$(MauiVersion)" />
				</ItemGroup>
				<ItemGroup>
					<TrimmerRootAssembly Include="Microsoft.Maui" />
					<TrimmerRootAssembly Include="Microsoft.Maui.Controls" />
					<TrimmerRootAssembly Include="Microsoft.Maui.Controls.Foldable" />
					<TrimmerRootAssembly Include="Microsoft.Maui.Controls.Maps" />
					<TrimmerRootAssembly Include="Microsoft.Maui.Controls.Xaml" />
					<TrimmerRootAssembly Include="Microsoft.Maui.Essentials" />
					<TrimmerRootAssembly Include="Microsoft.Maui.Graphics" />
					<TrimmerRootAssembly Include="Microsoft.Maui.Graphics.Skia" />
					<TrimmerRootAssembly Include="Microsoft.Maui.Maps" />
				</ItemGroup>
			</Project>
			""");

		string binLogFilePath = $"publish-{DateTime.UtcNow.ToFileTimeUtc()}.binlog";
		Assert.IsTrue(DotnetInternal.Build(projectFile, "Release", framework: framework, properties: extendedBuildProps, runtimeIdentifier: runtimeIdentifier, binlogPath: binLogFilePath),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

		var actualWarnings = BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog(binLogFilePath);
		var expectedWarnings = isWindowsFramework && BuildWarningsUtilities.ExpectedNativeAOTWarningsWindows != null
			? BuildWarningsUtilities.ExpectedNativeAOTWarningsWindows
			: BuildWarningsUtilities.ExpectedNativeAOTWarnings;
		actualWarnings.AssertWarnings(expectedWarnings);
	}

	private List<string> PrepareNativeAotBuildProps()
	{
		var extendedBuildProps = new List<string>(BuildProps)
		{
			"PublishAot=true",
			"PublishAotUsingRuntimePack=true",  // TODO: This parameter will become obsolete https://github.com/dotnet/runtime/issues/87060 in net9
			"_IsPublishing=true", // This makes 'dotnet build -r iossimulator-x64' equivalent to 'dotnet publish -r iossimulator-x64'
			"IlcTreatWarningsAsErrors=false",
			"TrimmerSingleWarn=false",
			"_RequireCodeSigning=false" // This is required to build the iOS app without a signing key
		};
		return extendedBuildProps;
	}

	private List<string> PrepareNativeAotBuildPropsWindows(string runtimeIdentifier)
	{
		var extendedBuildProps = new List<string>(BuildProps)
		{
			"PublishAot=true",
			"PublishAotUsingRuntimePack=true",
			"_IsPublishing=true",
			"IlcTreatWarningsAsErrors=false",
			"TrimmerSingleWarn=false",

			// Windows-specific properties
			$"RuntimeIdentifierOverride={runtimeIdentifier}",
			"WindowsPackageType=None",
			"SelfContained=true"
		};
		return extendedBuildProps;
	}
}
