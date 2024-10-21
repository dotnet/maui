using Microsoft.Maui.IntegrationTests.Apple;

namespace Microsoft.Maui.IntegrationTests;

[Category(Categories.macOSTemplates)]
public class MacTemplateTest : BaseTemplateTests
{
	[Test]
	[TestCase("maui", "ios")]
	[TestCase("maui", "maccatalyst")]
	[TestCase("maui-blazor", "ios")]
	[TestCase("maui-blazor", "maccatalyst")]
	public void BuildWithCustomBundleResource(string id, string framework)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, DotNetCurrent),
			$"Unable to create template {id}. Check test output for errors.");

		File.WriteAllText(Path.Combine(projectDir, "Resources", "testfile.txt"), "Something here :)");

		FileUtilities.ReplaceInFile(projectFile,
			"</Project>",
			$"""
			  <ItemGroup>
			    <BundleResource Include="Resources\testfile.txt" />
			  </ItemGroup>
			</Project>
			""");

		var extendedBuildProps = BuildProps;
		extendedBuildProps.Add($"TargetFramework={DotNetCurrent}-{framework}");

		Assert.IsTrue(DotnetInternal.Build(projectFile, "Debug", properties: extendedBuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Test]
	[TestCase("maui-blazor", "Debug", DotNetCurrent)]
	[TestCase("maui-blazor", "Release", DotNetCurrent)]
	public void CheckEntitlementsForMauiBlazorOnMacCatalyst(string id, string config, string framework)
	{
		if (TestEnvironment.IsWindows)
			Assert.Ignore("Running MacCatalyst templates is only supported on Mac.");

		string projectDir = TestDirectory;
		string projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");
		// Note: Debug app is stored in the maccatalyst-x64 folder, while the Release is in parent directory
		string appLocation = config == "Release" ?
			Path.Combine(projectDir, "bin", config, $"{framework}-maccatalyst", $"{Path.GetFileName(projectDir)}.app") :
			Path.Combine(projectDir, "bin", config, $"{framework}-maccatalyst", "maccatalyst-x64", $"{Path.GetFileName(projectDir)}.app");
		string entitlementsPath = Path.Combine(projectDir, "x.xml");

		List<string> buildWithCodeSignProps = new List<string>(BuildProps)
		{
			"EnableCodeSigning=true"
		};

		Assert.IsTrue(DotnetInternal.New(id, projectDir, framework), $"Unable to create template {id}. Check test output for errors.");
		Assert.IsTrue(DotnetInternal.Build(projectFile, config, framework: $"{framework}-maccatalyst", properties: buildWithCodeSignProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

		List<string> expectedEntitlements =
			new() { "com.apple.security.app-sandbox", "com.apple.security.network.client" };
		List<string> foundEntitlements = Codesign.SearchForExpectedEntitlements(entitlementsPath, appLocation, expectedEntitlements);

		CollectionAssert.AreEqual(expectedEntitlements, foundEntitlements, "Entitlements missing from executable.");
	}

	[Test]
	[TestCase("maui-blazor", "Debug", DotNetCurrent)]
	[TestCase("maui-blazor", "Release", DotNetCurrent)]
	[TestCase("maui", "Debug", DotNetCurrent)]
	[TestCase("maui", "Release", DotNetCurrent)]
	[TestCase("maui-multiproject", "Debug", DotNetCurrent)]
	[TestCase("maui-multiproject", "Release", DotNetCurrent)]
	public void CheckPrivacyManifestForiOS(string id, string config, string framework)
	{
		if (TestEnvironment.IsWindows)
		{
			Assert.Ignore("Running iOS templates is only supported on Mac.");
		}

		string projectDir = TestDirectory;
		string projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");
		string appFileName = $"{Path.GetFileName(projectDir)}.app";
		string appLocation =
			Path.Combine(projectDir, "bin", config, $"{framework}-ios", "iossimulator-x64", appFileName);

		// Multi-project is in a .iOS subfolder and csproj is *.iOS.csproj
		if (id.EndsWith("multiproject"))
		{
			projectFile =
				Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.iOS", $"{Path.GetFileName(projectDir)}.iOS.csproj");

			appFileName = $"{Path.GetFileName(projectDir)}.iOS.app";

			appLocation =
				Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.iOS", "bin", config, $"{framework}-ios", "iossimulator-x64", appFileName);
		}

		Assert.IsTrue(DotnetInternal.New(id, projectDir, framework), $"Unable to create template {id}. Check test output for errors.");
		Assert.IsTrue(DotnetInternal.Build(projectFile, config, framework: $"{framework}-ios", msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

		string manifestLocation = Path.Combine(appLocation, "PrivacyInfo.xcprivacy");

		Assert.IsTrue(File.Exists(manifestLocation), $"Privacy Manifest not found in {manifestLocation}.");
	}
}
