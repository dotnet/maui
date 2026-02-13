namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "AOT")]
public class AOTTemplateTest : BaseTemplateTests
{
	public AOTTemplateTest(IntegrationTestFixture fixture, ITestOutputHelper output) : base(fixture, output) { }

	[Theory]
	[InlineData("maui", $"{DotNetCurrent}-android", "android-arm64")]
	[InlineData("maui", $"{DotNetCurrent}-android", "android-x64")]
	[InlineData("maui", $"{DotNetCurrent}-ios", "ios-arm64")]
	[InlineData("maui", $"{DotNetCurrent}-ios", "iossimulator-arm64")]
	[InlineData("maui", $"{DotNetCurrent}-ios", "iossimulator-x64")]
	[InlineData("maui", $"{DotNetCurrent}-maccatalyst", "maccatalyst-arm64")]
	[InlineData("maui", $"{DotNetCurrent}-maccatalyst", "maccatalyst-x64")]
	[InlineData("maui", $"{DotNetCurrent}-windows10.0.19041.0", "win-x64")]
	[InlineData("maui", $"{DotNetCurrent}-windows10.0.19041.0", "win-arm64")]
	public void PublishNativeAOT(string id, string framework, string runtimeIdentifier)
	{
		SetTestIdentifier(id, framework, runtimeIdentifier);
		bool isWindowsFramework = framework.Contains("windows", StringComparison.OrdinalIgnoreCase);
		bool isApplePlatform = framework.Contains("ios", StringComparison.OrdinalIgnoreCase) || framework.Contains("maccatalyst", StringComparison.OrdinalIgnoreCase);
		bool isAndroidPlatform = framework.Contains("android", StringComparison.OrdinalIgnoreCase);

		if (isApplePlatform && !TestEnvironment.IsMacOS)
			if (true) return; // Skip: "Publishing a MAUI iOS/macOS app with NativeAOT is only supported on a host MacOS system."

		if (isWindowsFramework && !TestEnvironment.IsWindows)
			if (true) return; // Skip: "Publishing a MAUI Windows app with NativeAOT is only supported on a host Windows system."

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, DotNetCurrent, output: _output),
			$"Unable to create template {id}. Check test output for errors.");

		// For Android-only builds on Linux, modify the csproj to only target Android
		// This avoids restore failures due to missing iOS/macCatalyst workloads
		if (isAndroidPlatform && !TestEnvironment.IsMacOS && !TestEnvironment.IsWindows)
		{
			OnlyAndroid(projectFile);
		}

		var extendedBuildProps = isWindowsFramework
			? PrepareNativeAotBuildPropsWindows(runtimeIdentifier)
			: isAndroidPlatform
				? PrepareNativeAotBuildPropsAndroid()
				: PrepareNativeAotBuildProps();

		// Disable code signing for Apple platforms (no signing certificate available in CI)
		if (isApplePlatform)
		{
			AddNoCodeSigningProps(extendedBuildProps);
		}

		string binLogFilePath = $"publish-{DateTime.UtcNow.ToFileTimeUtc()}.binlog";
		Assert.True(DotnetInternal.Build(projectFile, "Release", framework: framework, properties: extendedBuildProps, runtimeIdentifier: runtimeIdentifier, binlogPath: binLogFilePath, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

		var actualWarnings = BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog(binLogFilePath);
		var expectedWarnings = isAndroidPlatform
			? BuildWarningsUtilities.ExpectedNativeAOTWarningsAndroid
			: isWindowsFramework
				? BuildWarningsUtilities.ExpectedNativeAOTWarningsWindows
				: BuildWarningsUtilities.ExpectedNativeAOTWarnings;
		actualWarnings.AssertWarnings(expectedWarnings);
	}

	[Theory]
	[InlineData("maui", $"{DotNetCurrent}-android", "android-arm64")]
	[InlineData("maui", $"{DotNetCurrent}-android", "android-x64")]
	[InlineData("maui", $"{DotNetCurrent}-ios", "ios-arm64")]
	[InlineData("maui", $"{DotNetCurrent}-ios", "iossimulator-arm64")]
	[InlineData("maui", $"{DotNetCurrent}-ios", "iossimulator-x64")]
	[InlineData("maui", $"{DotNetCurrent}-maccatalyst", "maccatalyst-arm64")]
	[InlineData("maui", $"{DotNetCurrent}-maccatalyst", "maccatalyst-x64")]
	[InlineData("maui", $"{DotNetCurrent}-windows10.0.19041.0", "win-x64")]
	[InlineData("maui", $"{DotNetCurrent}-windows10.0.19041.0", "win-arm64")]
	public void PublishNativeAOTRootAllMauiAssemblies(string id, string framework, string runtimeIdentifier)
	{
		// This test follows the following guide: https://devblogs.microsoft.com/dotnet/creating-aot-compatible-libraries/#publishing-a-test-application-for-aot
		bool isWindowsFramework = framework.Contains("windows", StringComparison.OrdinalIgnoreCase);
		bool isApplePlatform = framework.Contains("ios", StringComparison.OrdinalIgnoreCase) || framework.Contains("maccatalyst", StringComparison.OrdinalIgnoreCase);
		bool isAndroidPlatform = framework.Contains("android", StringComparison.OrdinalIgnoreCase);

		if (isApplePlatform && !TestEnvironment.IsMacOS)
			if (true) return; // Skip: "Publishing a MAUI iOS/macOS app with NativeAOT is only supported on a host MacOS system."

		if (isWindowsFramework && !TestEnvironment.IsWindows)
			if (true) return; // Skip: "Publishing a MAUI Windows app with NativeAOT is only supported on a host Windows system."

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, DotNetCurrent, output: _output),
			$"Unable to create template {id}. Check test output for errors.");

		// For Android-only builds on Linux, modify the csproj to only target Android
		// This avoids restore failures due to missing iOS/macCatalyst workloads
		if (isAndroidPlatform && !TestEnvironment.IsMacOS && !TestEnvironment.IsWindows)
		{
			OnlyAndroid(projectFile);
		}

		var extendedBuildProps = isWindowsFramework
			? PrepareNativeAotBuildPropsWindows(runtimeIdentifier)
			: isAndroidPlatform
				? PrepareNativeAotBuildPropsAndroid()
				: PrepareNativeAotBuildProps();

		// Disable code signing for Apple platforms (no signing certificate available in CI)
		if (isApplePlatform)
		{
			AddNoCodeSigningProps(extendedBuildProps);
		}

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
		Assert.True(DotnetInternal.Build(projectFile, "Release", framework: framework, properties: extendedBuildProps, runtimeIdentifier: runtimeIdentifier, binlogPath: binLogFilePath, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

		var actualWarnings = BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog(binLogFilePath);
		var expectedWarnings = isAndroidPlatform
			? BuildWarningsUtilities.ExpectedNativeAOTWarningsAndroid
			: isWindowsFramework
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
			"TrimmerSingleWarn=false"
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

	private List<string> PrepareNativeAotBuildPropsAndroid()
	{
		var extendedBuildProps = new List<string>(BuildProps)
		{
			"PublishAot=true",
			"PublishAotUsingRuntimePack=true",
			"_IsPublishing=true",
			"IlcTreatWarningsAsErrors=false",
			"TrimmerSingleWarn=false"
		};

		var ndkRoot = Environment.GetEnvironmentVariable("ANDROID_NDK_ROOT");
		if (!string.IsNullOrEmpty(ndkRoot))
		{
			// Quote and escape the NDK path to avoid argument splitting when it contains spaces.
			var ndkRootEscaped = ndkRoot.Replace("\"", "\\\"", StringComparison.Ordinal);
			extendedBuildProps.Add($"AndroidNdkDirectory=\"{ndkRootEscaped}\"");
		}

		return extendedBuildProps;
	}

	/// <summary>
	/// Adds properties to disable code signing for Apple platforms.
	/// This is required when building without a signing certificate (e.g., in CI environments).
	/// </summary>
	private static void AddNoCodeSigningProps(List<string> buildProps)
	{
		buildProps.Add("EnableCodeSigning=false");
		buildProps.Add("_RequireCodeSigning=false");
	}

}
