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
	[TestCase("maui-blazor", DotNetPrevious, "Debug", false, "", "")]
	[TestCase("maui-blazor", DotNetPrevious, "Release", false, "", "")]
	[TestCase("maui-blazor", DotNetCurrent, "Debug", false, "", "")]
	[TestCase("maui-blazor", DotNetCurrent, "Release", false, "", "TrimMode=partial")]
	[TestCase("maui-blazor", DotNetCurrent, "Debug", false, "--Empty", "")]
	[TestCase("maui-blazor", DotNetCurrent, "Release", false, "--Empty", "TrimMode=partial")]
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

		// TODO: remove this if as we should be able to build tizen net8
		if (framework != DotNetPrevious)
			EnableTizen(projectFile);

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
	// with invalid characters
	[TestCase("maui", "Project@Symbol", "projectsymbol")]
	[TestCase("maui-blazor", "Project@Symbol", "projectsymbol")]
	[TestCase("mauilib", "Project@Symbol", "projectsymbol")]
	public void BuildsWithSpecialCharacters(string id, string projectName, string expectedId)
	{
		var projectDir = Path.Combine(TestDirectory, projectName);
		var projectFile = Path.Combine(projectDir, $"{projectName}.csproj");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, DotNetCurrent),
			$"Unable to create template {id}. Check test output for errors.");

		EnableTizen(projectFile);

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

		// TODO: remove this if as we should be able to build tizen net8
		if (framework != DotNetPrevious)
			EnableTizen(projectFile);

		if (shouldPack)
			FileUtilities.ReplaceInFile(projectFile,
				"</Project>",
				"<PropertyGroup><Version>1.0.0-preview.1</Version></PropertyGroup></Project>");

		// set <MauiVersion> in the csproj as that is the reccommended place
		var mv = framework == DotNetPrevious ? MauiVersionPrevious : MauiVersionCurrent;
		FileUtilities.ReplaceInFile(projectFile,
			"</Project>",
			$"<PropertyGroup><MauiVersion>{mv}</MauiVersion></PropertyGroup></Project>");

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

		// TODO: fix this as we should be able to build tizen net8
		// EnableTizen(projectFile);

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

		EnableTizen(projectFile);
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

		// TODO: remove this if as we should be able to build tizen net8
		if (framework != DotNetPrevious)
			EnableTizen(projectFile);

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

		EnableTizen(projectFile);
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

		EnableTizen(projectFile);
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

	// This test is super temporary and is just for the interim
	// while we productize the CollectionViewHandler2. Once we
	// ship it as the default, this test will fail and can be deleted.
	[Test]
	[TestCase("maui", DotNetCurrent, "", false)]
	[TestCase("maui", DotNetCurrent, "--sample-content", true)]
	public void SampleShouldHaveHandler2Registered(string id, string framework, string additionalDotNetNewParams, bool shouldHaveHandler2)
	{
		var projectDir = TestDirectory;
		var programFile = Path.Combine(projectDir, "MauiProgram.cs");

		Assert.IsTrue(DotnetInternal.New(id, projectDir, framework, additionalDotNetNewParams),
			$"Unable to create template {id}. Check test output for errors.");

		var programContents = File.ReadAllText(programFile);

		if (shouldHaveHandler2)
		{
			AssertContains("#if IOS || MACCATALYST", programContents);
			AssertContains("handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();", programContents);
		}
		else
		{
			AssertDoesNotContain("#if IOS || MACCATALYST", programContents);
			AssertDoesNotContain("handlers.AddHandler<Microsoft.Maui.Controls.CollectionView, Microsoft.Maui.Controls.Handlers.Items2.CollectionViewHandler2>();", programContents);
		}
	}

	[Test]
	[TestCase("SentenceStudio.ServiceDefaults")]
	[TestCase("MyApp.ServiceDefaults")]
	[TestCase("ServiceDefaults")]
	public void AspireServiceDefaultsTemplateUsesCorrectProjectName(string projectName)
	{
		var projectDir = Path.Combine(TestDirectory, projectName);
		var expectedProjectFile = Path.Combine(projectDir, $"{projectName}.csproj");

		Assert.IsTrue(DotnetInternal.New("maui-aspire-servicedefaults", projectDir),
			$"Unable to create template maui-aspire-servicedefaults. Check test output for errors.");

		// Verify the project file has the correct name
		Assert.IsTrue(File.Exists(expectedProjectFile),
			$"Project file should be named {projectName}.csproj, but {expectedProjectFile} does not exist.");

		// Verify the project builds successfully
		Assert.IsTrue(DotnetInternal.Build(expectedProjectFile, "Debug", properties: BuildProps),
			$"Project {expectedProjectFile} failed to build. Check test output/attachments for errors.");

		// Verify namespace is correctly set in Extensions.cs
		var extensionsFile = Path.Combine(projectDir, "Extensions.cs");
		Assert.IsTrue(File.Exists(extensionsFile), "Extensions.cs should exist.");
		
		var extensionsContent = File.ReadAllText(extensionsFile);
		Assert.IsTrue(extensionsContent.Contains($"namespace {projectName};", StringComparison.Ordinal),
			$"Extensions.cs should contain correct namespace '{projectName}', but content was: {extensionsContent}");
	}
}
