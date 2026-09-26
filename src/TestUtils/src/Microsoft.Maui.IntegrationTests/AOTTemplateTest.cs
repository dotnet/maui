namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "AOT")]
public class AOTTemplateTest : BaseTemplateTests
{
	public AOTTemplateTest(IntegrationTestFixture fixture, ITestOutputHelper output) : base(fixture, output) { }

	[Theory]
	[MemberData(nameof(NativeAotPublishCases))]
	public void PublishNativeAOT(string id, string framework, string runtimeIdentifier, string options)
	{
		SetTestIdentifier(id, framework, runtimeIdentifier, options);
		bool isWindowsFramework = framework.Contains("windows", StringComparison.OrdinalIgnoreCase);
		bool isApplePlatform = framework.Contains("ios", StringComparison.OrdinalIgnoreCase) || framework.Contains("maccatalyst", StringComparison.OrdinalIgnoreCase);
		bool isAndroidPlatform = framework.Contains("android", StringComparison.OrdinalIgnoreCase);
		bool isSimulator = runtimeIdentifier.StartsWith("iossimulator-", StringComparison.Ordinal);

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, DotNetCurrent, $"{options} --no-restore", output: _output),
			$"Unable to create template {id}. Check test output for errors.");


		var extendedBuildProps = isWindowsFramework
			? PrepareNativeAotBuildPropsWindows(runtimeIdentifier)
			: isAndroidPlatform
				? PrepareNativeAotBuildPropsAndroid(BuildProps)
				: PrepareNativeAotBuildProps();

		// Disable code signing for Apple platforms (no signing certificate available in CI)
		if (isApplePlatform)
		{
			AddNoCodeSigningProps(extendedBuildProps);
			_output.WriteLine("Unsigned Apple compile coverage only; this does not verify signing, installation, or device execution.");
		}

		extendedBuildProps.Add($"TargetFrameworks={framework}");
		// Apple Native AOT selects full trimming itself; an explicit TrimMode bypasses its Full link mode.
		if (!isApplePlatform)
			extendedBuildProps.Add("TrimMode=full");
		if (!isSimulator)
		{
			extendedBuildProps.Remove("_IsPublishing=true");
			extendedBuildProps.Remove("PublishAotUsingRuntimePack=true");
		}
		if (isAndroidPlatform)
			_output.WriteLine("Android Native AOT is experimental, including when sample preview features suppress XA1040.");

		string binLogFilePath = Path.Combine(
			Path.GetDirectoryName(projectFile) ?? "",
			$"publish-{DateTime.UtcNow.ToFileTimeUtc()}.binlog");
		if (isSimulator)
		{
			_output.WriteLine("Supplemental iOS simulator build with internal publishing flags; not public Native AOT publish coverage.");
			Assert.True(DotnetInternal.Build(projectFile, "Release", framework: framework, properties: extendedBuildProps, runtimeIdentifier: runtimeIdentifier, binlogPath: binLogFilePath, output: _output),
				$"Project {Path.GetFileName(projectFile)} failed to build for the simulator.");
		}
		else
		{
			Assert.True(DotnetInternal.Publish(projectFile, "Release", framework: framework, properties: extendedBuildProps, runtimeIdentifier: runtimeIdentifier, binlogPath: binLogFilePath, output: _output),
				$"Project {Path.GetFileName(projectFile)} failed to publish. Check test output/attachments for errors.");
			BuildWarningsUtilities.AssertTargetSucceeded(binLogFilePath, projectFile, "Publish");
		}
		BuildWarningsUtilities.AssertProjectProperties(binLogFilePath, projectFile, framework,
			("PublishAot", "true"), ("PublishTrimmed", "true"), ("TrimMode", "full"));
		BuildWarningsUtilities.AssertTaskSucceededInTarget(binLogFilePath, projectFile, "IlcCompile", "Exec");

		var actualWarnings = BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog(binLogFilePath);
		var androidPreview = isAndroidPlatform && options == "--sample-content";
		var expectedWarnings = isAndroidPlatform && (!androidPreview || actualWarnings.Any(file => file.WarningsPerCode.Any(warning => warning.Code == "XA1040")))
			? BuildWarningsUtilities.ExpectedNativeAOTWarningsAndroid
			: isWindowsFramework
				? BuildWarningsUtilities.ExpectedNativeAOTWarningsWindows
				: BuildWarningsUtilities.ExpectedNativeAOTWarnings;
		actualWarnings.AssertWarnings(expectedWarnings);
	}

	public static IEnumerable<object[]> NativeAotPublishCases()
	{
		foreach (var testCase in FullTrimPublishCases())
			yield return testCase;

		// Keep the existing simulator workaround supplemental, without multiplying it across app options.
		if (TestEnvironment.IsMacOS)
		{
			yield return new object[] { "maui", $"{DotNetCurrent}-ios", "iossimulator-arm64", "" };
			yield return new object[] { "maui", $"{DotNetCurrent}-ios", "iossimulator-x64", "" };
		}
	}

	public static IEnumerable<object[]> FullTrimPublishCases()
	{
		var profiles = new List<(string platform, string rid)>
		{
			("android", "android-arm64"),
			("android", "android-x64"),
		};
		if (TestEnvironment.IsMacOS)
		{
			profiles.Add(("ios", "ios-arm64"));
			profiles.Add(("maccatalyst", "maccatalyst-arm64"));
			profiles.Add(("maccatalyst", "maccatalyst-x64"));
		}
		if (TestEnvironment.IsWindows)
		{
			profiles.Add(("windows10.0.19041.0", "win-x64"));
			profiles.Add(("windows10.0.19041.0", "win-arm64"));
		}

		foreach (var (platform, rid) in profiles)
			foreach (var options in new[] { "", "--ui csharp", "--sample-content" })
				yield return new object[] { "maui", $"{DotNetCurrent}-{platform}", rid, options };
	}

	[Theory]
	[MemberData(nameof(FullTrimPublishCases))]
	public void PublishFullTrim(string id, string framework, string runtimeIdentifier, string options)
	{
		SetTestIdentifier(id, framework, runtimeIdentifier, options);
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");
		Assert.True(DotnetInternal.New(id, projectDir, DotNetCurrent, $"{options} --no-restore", output: _output),
			$"Unable to create template {id} with '{options}'.");

		var buildProps = BuildProps;
		buildProps.Add($"TargetFrameworks={framework}");
		buildProps.Add("TrimMode=full");
		var isApple = framework.Contains("-ios", StringComparison.Ordinal) || framework.Contains("-maccatalyst", StringComparison.Ordinal);
		if (isApple)
		{
			// The Apple SDK computes PublishTrimmed itself; keep the existing unsigned CI compile contract.
			AddNoCodeSigningProps(buildProps);
			_output.WriteLine("Unsigned Apple publish coverage only; no signed/device execution claim.");
		}
		else
			buildProps.Add("PublishTrimmed=true");
		if (framework.Contains("-windows", StringComparison.Ordinal))
		{
			buildProps.Add("WindowsPackageType=None");
			buildProps.Add("SelfContained=true");
			buildProps.Add($"RuntimeIdentifierOverride={runtimeIdentifier}");
		}

		var binlog = Path.Combine(projectDir, "publish-full-trim.binlog");
		Assert.True(DotnetInternal.Publish(projectFile, "Release", framework: framework, properties: buildProps,
			runtimeIdentifier: runtimeIdentifier, binlogPath: binlog, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed full-trim publication.");
		if (isApple)
		{
			// _ComputePublishTrimmed sets PublishTrimmed during execution, not project evaluation.
			BuildWarningsUtilities.AssertProjectProperties(binlog, projectFile, framework, ("TrimMode", "full"));
		}
		else
		{
			BuildWarningsUtilities.AssertProjectProperties(binlog, projectFile, framework,
				("PublishTrimmed", "true"), ("TrimMode", "full"));
		}
		BuildWarningsUtilities.AssertTaskSucceededInTarget(binlog, projectFile, "_RunILLink", "ILLink", ("TrimMode", "full"));
		// The SDK's ILLink target is conditional on PublishTrimmed=true.
		BuildWarningsUtilities.AssertTargetSucceeded(binlog, projectFile, "ILLink");
		BuildWarningsUtilities.AssertTargetSucceeded(binlog, projectFile, "Publish");
		BuildWarningsUtilities.ReadNativeAOTWarningsFromBinLog(binlog).AssertNoWarnings();
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
			if (true)
				return; // Skip: "Publishing a MAUI iOS/macOS app with NativeAOT is only supported on a host MacOS system."

		if (isWindowsFramework && !TestEnvironment.IsWindows)
			if (true)
				return; // Skip: "Publishing a MAUI Windows app with NativeAOT is only supported on a host Windows system."

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, DotNetCurrent, output: _output),
			$"Unable to create template {id}. Check test output for errors.");


		var extendedBuildProps = isWindowsFramework
			? PrepareNativeAotBuildPropsWindows(runtimeIdentifier)
			: isAndroidPlatform
				? PrepareNativeAotBuildPropsAndroid(BuildProps)
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

		string binLogFilePath = Path.Combine(
			Path.GetDirectoryName(projectFile) ?? "",
			$"publish-{DateTime.UtcNow.ToFileTimeUtc()}.binlog");
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

	internal static List<string> PrepareNativeAotBuildPropsAndroid(List<string> buildProps)
	{
		var extendedBuildProps = new List<string>(buildProps)
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
