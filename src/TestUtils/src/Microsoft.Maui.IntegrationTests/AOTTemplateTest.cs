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
	public void PublishNativeAOT(string id, string framework, string runtimeIdentifier)
	{
		if (!TestEnvironment.IsMacOS)
			Assert.Ignore("Publishing a MAUI iOS app with NativeAOT is only supported on a host MacOS system.");

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, DotNetCurrent),
			$"Unable to create template {id}. Check test output for errors.");

		var extendedBuildProps = PrepareNativeAotBuildProps();

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
	public void PublishNativeAOTRootAllMauiAssemblies(string id, string framework, string runtimeIdentifier)
	{
		// This test follows the following guide: https://devblogs.microsoft.com/dotnet/creating-aot-compatible-libraries/#publishing-a-test-application-for-aot
		if (!TestEnvironment.IsMacOS)
			Assert.Ignore("Publishing a MAUI iOS app with NativeAOT is only supported on a host MacOS system.");

		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, DotNetCurrent),
			$"Unable to create template {id}. Check test output for errors.");

		var extendedBuildProps = PrepareNativeAotBuildProps();
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
		actualWarnings.AssertWarnings(BuildWarningsUtilities.ExpectedNativeAOTWarnings);
	}

	private List<string> PrepareNativeAotBuildProps()
	{
		var extendedBuildProps = new List<string>(BuildProps);
		extendedBuildProps.Add("PublishAot=true");
		extendedBuildProps.Add("PublishAotUsingRuntimePack=true");  // TODO: This parameter will become obsolete https://github.com/dotnet/runtime/issues/87060 in net9
		extendedBuildProps.Add("_IsPublishing=true"); // This makes 'dotnet build -r iossimulator-x64' equivalent to 'dotnet publish -r iossimulator-x64'
		extendedBuildProps.Add("IlcTreatWarningsAsErrors=false");
		extendedBuildProps.Add("TrimmerSingleWarn=false");
		extendedBuildProps.Add("_RequireCodeSigning=false"); // This is required to build the iOS app without a signing key
		return extendedBuildProps;
	}
}
