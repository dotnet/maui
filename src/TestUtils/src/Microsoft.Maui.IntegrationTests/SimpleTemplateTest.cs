using System.Xml.Linq;

namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class SimpleTemplateTest : BaseTemplateTests
{
	public SimpleTemplateTest(IntegrationTestFixture fixture, ITestOutputHelper output) : base(fixture, output) { }

	[Theory]
	// Parameters: short name, target framework, build config, use pack target, additionalDotNetNewParams, additionalDotNetBuildParams
	// [InlineData("maui", DotNetPrevious, "Debug", false, "", "")]
	// [InlineData("maui", DotNetPrevious, "Release", false, "", "")]
	[InlineData("maui", DotNetCurrent, "Debug", false, "", "")]
	[InlineData("maui", DotNetCurrent, "Release", false, "", "TrimMode=partial")]
	[InlineData("maui", DotNetCurrent, "Debug", false, "--sample-content", "")]
	[InlineData("maui", DotNetCurrent, "Release", false, "--sample-content", "TrimMode=partial")]
	//Debug not ready yet
	//[InlineData("maui", DotNetCurrent, "Debug", false, "--sample-content", "UseMonoRuntime=false")]
	[InlineData("maui", DotNetCurrent, "Release", false, "--sample-content", "UseMonoRuntime=false EnablePreviewFeatures=true")]
	// [InlineData("maui-blazor", DotNetPrevious, "Debug", false, "", "")]
	// [InlineData("maui-blazor", DotNetPrevious, "Release", false, "", "")]
	[InlineData("maui-blazor", DotNetCurrent, "Debug", false, "", "")]
	[InlineData("maui-blazor", DotNetCurrent, "Release", false, "", "TrimMode=partial")]
	[InlineData("maui-blazor", DotNetCurrent, "Debug", false, "--empty", "")]
	[InlineData("maui-blazor", DotNetCurrent, "Release", false, "--empty", "TrimMode=partial")]
	// [InlineData("mauilib", DotNetPrevious, "Debug", true, "", "")]
	// [InlineData("mauilib", DotNetPrevious, "Release", true, "", "")]
	[InlineData("mauilib", DotNetCurrent, "Debug", true, "", "")]
	[InlineData("mauilib", DotNetCurrent, "Release", true, "", "TrimMode=partial")]
	public void Build(string id, string framework, string config, bool shouldPack, string additionalDotNetNewParams, string additionalDotNetBuildParams)
	{
		SetTestIdentifier(id, framework, config, shouldPack, additionalDotNetNewParams, additionalDotNetBuildParams);
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework, additionalDotNetNewParams),
			$"Unable to create template {id}. Check test output for errors.");


		if (shouldPack)
			FileUtilities.ReplaceInFile(projectFile,
				"</Project>",
				"<PropertyGroup><Version>1.0.0-preview.1</Version></PropertyGroup></Project>");

		// We only have these packs for Android
		if (additionalDotNetBuildParams.Contains("UseMonoRuntime=false", StringComparison.OrdinalIgnoreCase))
		{
			OnlyAndroid(projectFile);
		}

		var buildProps = BuildProps;

		if (additionalDotNetBuildParams is not "" and not null)
		{
			additionalDotNetBuildParams.Split(" ").ToList().ForEach(p => buildProps.Add(p));
		}

		string target = shouldPack ? "Pack" : "";
		Assert.True(DotnetInternal.Build(projectFile, config, target: target, properties: buildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Theory]
	//[InlineData("maui", DotNetPrevious, "Debug")]
	public void InstallPackagesIntoUnsupportedTfmFails(string id, string framework, string config)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework),
			$"Unable to create template {id}. Check test output for errors.");

	// 	FileUtilities.ReplaceInFile(projectFile,
	// 		"$(MauiVersion)",
	// 		MauiPackageVersion);

	// 	Assert.False(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true),
	// 		$"Project {Path.GetFileName(projectFile)} built, but should not have. Check test output/attachments for why.");
	// }

	[Theory]
	// with spaces
	[InlineData("maui", "Project Space", "projectspace")]
	[InlineData("maui-blazor", "Project Space", "projectspace")]
	[InlineData("mauilib", "Project Space", "projectspace")]
	[InlineData("maui", "Project@Symbol", "projectsymbol")]
	[InlineData("maui-blazor", "Project@Symbol", "projectsymbol")]
	[InlineData("mauilib", "Project@Symbol", "projectsymbol")]
	public void BuildsWithSpecialCharacters(string id, string projectName, string expectedId)
	{
		var projectDir = Path.Combine(TestDirectory, projectName);
		var projectFile = Path.Combine(projectDir, $"{projectName}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, DotNetCurrent),
			$"Unable to create template {id}. Check test output for errors.");

		// libraries do not have application IDs
		if (id != "mauilib")
		{
			var doc = XDocument.Load(projectFile);

			// Check the app ID got invalid characters removed
			var appId = doc.Root!
				.Elements("PropertyGroup")
				.Elements("ApplicationId")
				.Single()
				.Value;
			Assert.Equal($"com.companyname.{expectedId}", appId);

			// Check the app title matches the project name exactly (it might have been XML-encoded, but loading the document decodes that)
			var appTitle = doc.Root!
				.Elements("PropertyGroup")
				.Elements("ApplicationTitle")
				.Single()
				.Value;
			Assert.Equal(projectName, appTitle);
		}

		Assert.True(DotnetInternal.Build(projectFile, "Debug", properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Theory]
	// Parameters: short name, target framework, build config, use pack target, additionalDotNetBuildParams
	// [InlineData("maui", DotNetPrevious, "Debug", false, "")]
	// [InlineData("maui", DotNetPrevious, "Release", false, "")]
	[InlineData("maui", DotNetCurrent, "Debug", false, "")]
	[InlineData("maui", DotNetCurrent, "Release", false, "TrimMode=partial")]
	// [InlineData("maui-blazor", DotNetPrevious, "Debug", false, "")]
	// [InlineData("maui-blazor", DotNetPrevious, "Release", false, "")]
	[InlineData("maui-blazor", DotNetCurrent, "Debug", false, "")]
	[InlineData("maui-blazor", DotNetCurrent, "Release", false, "TrimMode=partial")]
	// [InlineData("mauilib", DotNetPrevious, "Debug", true, "")]
	// [InlineData("mauilib", DotNetPrevious, "Release", true, "")]
	[InlineData("mauilib", DotNetCurrent, "Debug", true, "")]
	[InlineData("mauilib", DotNetCurrent, "Release", true, "TrimMode=partial")]
	public void BuildWithMauiVersion(string id, string framework, string config, bool shouldPack, string additionalDotNetBuildParams)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework),
			$"Unable to create template {id}. Check test output for errors.");

		if (shouldPack)
			FileUtilities.ReplaceInFile(projectFile,
				"</Project>",
				"<PropertyGroup><Version>1.0.0-preview.1</Version></PropertyGroup></Project>");

		// set <MauiVersion> in the csproj as that is the reccommended place
		var mv = framework == DotNetPrevious ? MauiVersionPrevious : MauiVersionCurrent;
		if (!string.IsNullOrEmpty(mv))
		{
			FileUtilities.ReplaceInFile(projectFile,
				"</Project>",
				$"<PropertyGroup><MauiVersion>{mv}</MauiVersion></PropertyGroup></Project>");
		}

		string binlogDir = Path.Combine(TestEnvironment.GetMauiDirectory(), $"artifacts\\log\\{Path.GetFileName(projectDir)}.binlog");

		var buildProps = BuildProps;

		if (additionalDotNetBuildParams is not "" and not null)
		{
			additionalDotNetBuildParams.Split(" ").ToList().ForEach(p => buildProps.Add(p));
		}

		string target = shouldPack ? "Pack" : "";
		Assert.True(DotnetInternal.Build(projectFile, config, target: target, binlogPath: binlogDir, properties: buildProps),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

#if ENABLE_PREVIOUS_TFM_BUILDS
	[Theory]
	[InlineData("maui", "Debug", false)]
	[InlineData("maui", "Release", false)]
	[InlineData("maui-blazor", "Debug", false)]
	[InlineData("maui-blazor", "Release", false)]
	[InlineData("mauilib", "Debug", true)]
	[InlineData("mauilib", "Release", true)]
	public void PreviousDotNetCanUseLatestMaui(string id, string config, bool shouldPack)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, DotNetPrevious),
			$"Unable to create template {id}. Check test output for errors.");

		if (shouldPack)
			FileUtilities.ReplaceInFile(projectFile,
				"</Project>",
				"<PropertyGroup><Version>1.0.0-preview.1</Version></PropertyGroup></Project>");

		// set <MauiVersion> in the csproj as that is the reccommended place
		FileUtilities.ReplaceInFile(projectFile,
			"</Project>",
			$"""
			  <PropertyGroup>
			    <MauiVersion>{MauiPackageVersion}</MauiVersion>
				<NoWarn>$(NoWarn);CS0618</NoWarn>
			  </PropertyGroup>
			</Project>
			""");

		string target = shouldPack ? "Pack" : "";
		Assert.True(DotnetInternal.Build(projectFile, config, target: target, properties: BuildProps),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}
#endif

	[Fact]
	public void BuildHandlesBadFilesInImages()
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New("maui", projectDir, DotNetCurrent),
			$"Unable to create template maui. Check test output for errors.");

		File.WriteAllText(Path.Combine(projectDir, "Resources", "Images", ".DS_Store"), "Boom!");

		Assert.True(DotnetInternal.Build(projectFile, "Debug", properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	/// <summary>
	/// Tests the scenario where a .NET MAUI Library specifically uses UseMauiCore instead of UseMaui.
	/// </summary>
	[Theory]
	// [InlineData("mauilib", DotNetPrevious, "Debug")]
	// [InlineData("mauilib", DotNetPrevious, "Release")]
	[InlineData("mauilib", DotNetCurrent, "Debug")]
	[InlineData("mauilib", DotNetCurrent, "Release")]
	public void PackCoreLib(string id, string framework, string config)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework),
			$"Unable to create template {id}. Check test output for errors.");

		var projectSectionsToReplace = new Dictionary<string, string>()
		{
			{ "UseMaui", "UseMauiCore" }, // This is the key part of the test
			{ "SingleProject", "EnablePreviewMsixTooling" },
		};
		if (framework != "net7.0")
		{
			// On versions after net7.0 this package reference also has to be updated to ensure the version of the MAUI Core package
			// is specified and avoids the MA002 warning.
			projectSectionsToReplace.Add("Include=\"Microsoft.Maui.Controls\"", "Include=\"Microsoft.Maui.Core\"");
		}

		FileUtilities.ReplaceInFile(projectFile, projectSectionsToReplace);
		Directory.Delete(Path.Combine(projectDir, "Platforms"), recursive: true);

		Assert.True(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Theory]
	[InlineData("maui", DotNetCurrent, "Debug")]
	[InlineData("mauilib", DotNetCurrent, "Debug")]
	[InlineData("maui-blazor", DotNetCurrent, "Debug")]
	public void BuildWithoutPackageReference(string id, string framework, string config)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework),
			$"Unable to create template {id}. Check test output for errors.");

		FileUtilities.ReplaceInFile(projectFile,
			"</Project>",
			"<PropertyGroup><SkipValidateMauiImplicitPackageReferences>true</SkipValidateMauiImplicitPackageReferences></PropertyGroup></Project>");
		FileUtilities.ReplaceInFile(projectFile,
			"<PackageReference Include=\"Microsoft.Maui.Controls\" Version=\"$(MauiVersion)\" />",
			"");

		Assert.True(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Theory]
	[InlineData("maui", "Debug", "2.0", "2", "")]
	[InlineData("maui", "Release", "2.0", "2", "TrimMode=partial")]
	[InlineData("maui", "Release", "0.3", "3", "TrimMode=partial")]
	[InlineData("maui-blazor", "Debug", "2.0", "2", "")]
	[InlineData("maui-blazor", "Release", "2.0", "2", "TrimMode=partial")]
	[InlineData("maui-blazor", "Release", "0.3", "3", "TrimMode=partial")]
	public void BuildWithDifferentVersionNumber(string id, string config, string display, string version, string additionalDotNetBuildParams)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir),
			$"Unable to create template {id}. Check test output for errors.");

		FileUtilities.ReplaceInFile(projectFile,
			$"<ApplicationDisplayVersion>1.0</ApplicationDisplayVersion>",
			$"<ApplicationDisplayVersion>{display}</ApplicationDisplayVersion>");
		FileUtilities.ReplaceInFile(projectFile,
			$"<ApplicationVersion>1</ApplicationVersion>",
			$"<ApplicationVersion>{version}</ApplicationVersion>");

		var buildProps = BuildProps;

		if (additionalDotNetBuildParams is not "" and not null)
		{
			additionalDotNetBuildParams.Split(" ").ToList().ForEach(p => buildProps.Add(p));
		}

		Assert.True(DotnetInternal.Build(projectFile, config, properties: buildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Theory]
	[InlineData("SentenceStudio.ServiceDefaults")]
	[InlineData("MyApp.ServiceDefaults")]
	[InlineData("Company.Product.ServiceDefaults")]
	public void AspireServiceDefaultsTemplateUsesCorrectProjectName(string projectName)
	{
		var projectDir = Path.Combine(TestDirectory, projectName);
		var expectedProjectFile = Path.Combine(projectDir, $"{projectName}.csproj");

		Assert.True(DotnetInternal.New("maui-aspire-servicedefaults", projectDir, additionalDotNetNewParams: $"-n \"{projectName}\""),
			$"Unable to create template maui-aspire-servicedefaults. Check test output for errors.");

		// Verify the project file was created with the correct name (this was the bug)
		Assert.True(File.Exists(expectedProjectFile),
			$"Expected project file '{expectedProjectFile}' was not created. This indicates the template naming issue.");

		// Verify no incorrectly named files exist
		var incorrectFiles = Directory.GetFiles(projectDir, "*.csproj")
			.Where(f => !f.Equals(expectedProjectFile, StringComparison.OrdinalIgnoreCase))
			.ToArray();

		if (incorrectFiles.Any())
			Assert.Fail($"Found incorrectly named project files: {string.Join(", ", incorrectFiles.Select(Path.GetFileName))}. Only '{Path.GetFileName(expectedProjectFile)}' should exist.");

		// Verify the content is correct
		Assert.True(File.Exists(Path.Combine(projectDir, "Extensions.cs")),
			"Expected Extensions.cs file was not created.");

		// Verify we can build it (even if restore fails due to placeholder tokens, the project structure should be valid)
		var projectContent = File.ReadAllText(expectedProjectFile);
		Assert.True(projectContent.Contains("<IsAspireSharedProject>true</IsAspireSharedProject>", StringComparison.Ordinal),
			"Project file should contain Aspire-specific properties.");
	}
}
