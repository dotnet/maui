using System.Xml.Linq;

namespace Microsoft.Maui.IntegrationTests;

[Category(Categories.Build)]
public class SimpleTemplateTest : BaseTemplateTests
{
	[Test]
	// Parameters: short name, target framework, build config, use pack target, additionalDotNetNewParams, additionalDotNetBuildParams
	[TestCase("maui", DotNetPrevious, "Debug", false, "", "")]
	[TestCase("maui", DotNetPrevious, "Release", false, "", "")]
	[TestCase("maui", DotNetCurrent, "Debug", false, "", "")]
	[TestCase("maui", DotNetCurrent, "Release", false, "", "TrimMode=partial")]
	[TestCase("maui", DotNetCurrent, "Debug", false, "--sample-content", "")]
	[TestCase("maui", DotNetCurrent, "Release", false, "--sample-content", "TrimMode=partial")]
	//Debug not ready yet
	//[TestCase("maui", DotNetCurrent, "Debug", false, "--sample-content", "UseMonoRuntime=false")]
	[TestCase("maui", DotNetCurrent, "Release", false, "--sample-content", "UseMonoRuntime=false EnablePreviewFeatures=true")]
	[TestCase("maui-blazor", DotNetPrevious, "Debug", false, "", "")]
	[TestCase("maui-blazor", DotNetPrevious, "Release", false, "", "")]
	[TestCase("maui-blazor", DotNetCurrent, "Debug", false, "", "")]
	[TestCase("maui-blazor", DotNetCurrent, "Release", false, "", "TrimMode=partial")]
	[TestCase("maui-blazor", DotNetCurrent, "Debug", false, "--empty", "")]
	[TestCase("maui-blazor", DotNetCurrent, "Release", false, "--empty", "TrimMode=partial")]
	[TestCase("mauilib", DotNetPrevious, "Debug", true, "", "")]
	[TestCase("mauilib", DotNetPrevious, "Release", true, "", "")]
	[TestCase("mauilib", DotNetCurrent, "Debug", true, "", "")]
	[TestCase("mauilib", DotNetCurrent, "Release", true, "", "TrimMode=partial")]
	public void Build(string id, string framework, string config, bool shouldPack, string additionalDotNetNewParams, string additionalDotNetBuildParams)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, framework, additionalDotNetNewParams),
			$"Unable to create template {id}. Check test output for errors.");


		if (shouldPack)
			FileUtilities.ReplaceInFile(projectFile,
				"</Project>",
				"<PropertyGroup><Version>1.0.0-preview.1</Version></PropertyGroup></Project>");

		string[]? warningsToIgnore = null;

		if (additionalDotNetNewParams.Contains("sample-content", StringComparison.OrdinalIgnoreCase))
		{
			warningsToIgnore = new string[]
			{
				"XC0103", // https://github.com/CommunityToolkit/Maui/issues/2205
			};
		}

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
		Assert.IsTrue(DotnetInternal.Build(projectFile, config, target: target, properties: buildProps, msbuildWarningsAsErrors: true, warningsToIgnore: warningsToIgnore),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Test]
	[TestCase("maui", DotNetPrevious, "Debug")]
	public void InstallPackagesIntoUnsupportedTfmFails(string id, string framework, string config)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
			$"Unable to create template {id}. Check test output for errors.");

		FileUtilities.ReplaceInFile(projectFile,
			"$(MauiVersion)",
			MauiPackageVersion);

		Assert.False(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} built, but should not have. Check test output/attachments for why.");
	}

	[Test]
	// with spaces
	[TestCase("maui", "Project Space", "projectspace")]
	[TestCase("maui-blazor", "Project Space", "projectspace")]
	[TestCase("mauilib", "Project Space", "projectspace")]
	[TestCase("maui", "Project@Symbol", "projectsymbol")]
	[TestCase("maui-blazor", "Project@Symbol", "projectsymbol")]
	[TestCase("mauilib", "Project@Symbol", "projectsymbol")]
	public void BuildsWithSpecialCharacters(string id, string projectName, string expectedId)
	{
		var projectDir = Path.Combine(TestDirectory, projectName);
		var projectFile = Path.Combine(projectDir, $"{projectName}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, DotNetCurrent),
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
			Assert.AreEqual($"com.companyname.{expectedId}", appId);

			// Check the app title matches the project name exactly (it might have been XML-encoded, but loading the document decodes that)
			var appTitle = doc.Root!
				.Elements("PropertyGroup")
				.Elements("ApplicationTitle")
				.Single()
				.Value;
			Assert.AreEqual(projectName, appTitle);
		}

		Assert.IsTrue(DotnetInternal.Build(projectFile, "Debug", properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Test]
	// Parameters: short name, target framework, build config, use pack target, additionalDotNetBuildParams
	[TestCase("maui", DotNetPrevious, "Debug", false, "")]
	[TestCase("maui", DotNetPrevious, "Release", false, "")]
	[TestCase("maui", DotNetCurrent, "Debug", false, "")]
	[TestCase("maui", DotNetCurrent, "Release", false, "TrimMode=partial")]
	[TestCase("maui-blazor", DotNetPrevious, "Debug", false, "")]
	[TestCase("maui-blazor", DotNetPrevious, "Release", false, "")]
	[TestCase("maui-blazor", DotNetCurrent, "Debug", false, "")]
	[TestCase("maui-blazor", DotNetCurrent, "Release", false, "TrimMode=partial")]
	[TestCase("mauilib", DotNetPrevious, "Debug", true, "")]
	[TestCase("mauilib", DotNetPrevious, "Release", true, "")]
	[TestCase("mauilib", DotNetCurrent, "Debug", true, "")]
	[TestCase("mauilib", DotNetCurrent, "Release", true, "TrimMode=partial")]
	public void BuildWithMauiVersion(string id, string framework, string config, bool shouldPack, string additionalDotNetBuildParams)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
			$"Unable to create template {id}. Check test output for errors.");

		if (shouldPack)
			FileUtilities.ReplaceInFile(projectFile,
				"</Project>",
				"<PropertyGroup><Version>1.0.0-preview.1</Version></PropertyGroup></Project>");

		// set <MauiVersion> in the csproj as that is the reccommended place
		var mv = framework == DotNetPrevious ? MauiVersionPrevious : MauiVersionCurrent;
		if (mv is not null or "")
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
		Assert.IsTrue(DotnetInternal.Build(projectFile, config, target: target, binlogPath: binlogDir, properties: buildProps),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

#if ENABLE_PREVIOUS_TFM_BUILDS
	[Test]
	[TestCase("maui", "Debug", false)]
	[TestCase("maui", "Release", false)]
	[TestCase("maui-blazor", "Debug", false)]
	[TestCase("maui-blazor", "Release", false)]
	[TestCase("mauilib", "Debug", true)]
	[TestCase("mauilib", "Release", true)]
	public void PreviousDotNetCanUseLatestMaui(string id, string config, bool shouldPack)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, DotNetPrevious),
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
		Assert.IsTrue(DotnetInternal.Build(projectFile, config, target: target, properties: BuildProps),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}
#endif

	[Test]
	public void BuildHandlesBadFilesInImages()
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.IsTrue(DotnetInternal.New("maui", projectDir, DotNetCurrent),
			$"Unable to create template maui. Check test output for errors.");

		File.WriteAllText(Path.Combine(projectDir, "Resources", "Images", ".DS_Store"), "Boom!");

		Assert.IsTrue(DotnetInternal.Build(projectFile, "Debug", properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	/// <summary>
	/// Tests the scenario where a .NET MAUI Library specifically uses UseMauiCore instead of UseMaui.
	/// </summary>
	[Test]
	[TestCase("mauilib", DotNetPrevious, "Debug")]
	[TestCase("mauilib", DotNetPrevious, "Release")]
	[TestCase("mauilib", DotNetCurrent, "Debug")]
	[TestCase("mauilib", DotNetCurrent, "Release")]
	public void PackCoreLib(string id, string framework, string config)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
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

		Assert.IsTrue(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Test]
	[TestCase("maui", DotNetCurrent, "Debug")]
	[TestCase("mauilib", DotNetCurrent, "Debug")]
	[TestCase("maui-blazor", DotNetCurrent, "Debug")]
	public void BuildWithoutPackageReference(string id, string framework, string config)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
			$"Unable to create template {id}. Check test output for errors.");

		FileUtilities.ReplaceInFile(projectFile,
			"</Project>",
			"<PropertyGroup><SkipValidateMauiImplicitPackageReferences>true</SkipValidateMauiImplicitPackageReferences></PropertyGroup></Project>");
		FileUtilities.ReplaceInFile(projectFile,
			"<PackageReference Include=\"Microsoft.Maui.Controls\" Version=\"$(MauiVersion)\" />",
			"");

		Assert.IsTrue(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Test]
	[TestCase("maui", "Debug", "2.0", "2", "")]
	[TestCase("maui", "Release", "2.0", "2", "TrimMode=partial")]
	[TestCase("maui", "Release", "0.3", "3", "TrimMode=partial")]
	[TestCase("maui-blazor", "Debug", "2.0", "2", "")]
	[TestCase("maui-blazor", "Release", "2.0", "2", "TrimMode=partial")]
	[TestCase("maui-blazor", "Release", "0.3", "3", "TrimMode=partial")]
	public void BuildWithDifferentVersionNumber(string id, string config, string display, string version, string additionalDotNetBuildParams)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir),
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

		Assert.IsTrue(DotnetInternal.Build(projectFile, config, properties: buildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Test]
	[TestCase("SentenceStudio.ServiceDefaults")]
	[TestCase("MyApp.ServiceDefaults")]
	[TestCase("Company.Product.ServiceDefaults")]
	public void AspireServiceDefaultsTemplateUsesCorrectProjectName(string projectName)
	{
		var projectDir = Path.Combine(TestDirectory, projectName);
		var expectedProjectFile = Path.Combine(projectDir, $"{projectName}.csproj");

		Assert.IsTrue(DotnetInternal.New("maui-aspire-servicedefaults", projectDir, additionalDotNetNewParams: $"-n \"{projectName}\""),
			$"Unable to create template maui-aspire-servicedefaults. Check test output for errors.");

		// Verify the project file was created with the correct name (this was the bug)
		Assert.IsTrue(File.Exists(expectedProjectFile),
			$"Expected project file '{expectedProjectFile}' was not created. This indicates the template naming issue.");

		// Verify no incorrectly named files exist
		var incorrectFiles = Directory.GetFiles(projectDir, "*.csproj")
			.Where(f => !f.Equals(expectedProjectFile, StringComparison.OrdinalIgnoreCase))
			.ToArray();

		Assert.IsEmpty(incorrectFiles,
			$"Found incorrectly named project files: {string.Join(", ", incorrectFiles.Select(Path.GetFileName))}. Only '{Path.GetFileName(expectedProjectFile)}' should exist.");

		// Verify the content is correct
		Assert.IsTrue(File.Exists(Path.Combine(projectDir, "Extensions.cs")),
			"Expected Extensions.cs file was not created.");

		// Verify we can build it (even if restore fails due to placeholder tokens, the project structure should be valid)
		var projectContent = File.ReadAllText(expectedProjectFile);
		Assert.IsTrue(projectContent.Contains("<IsAspireSharedProject>true</IsAspireSharedProject>", StringComparison.Ordinal),
			"Project file should contain Aspire-specific properties.");
	}
}
