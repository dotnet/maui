using System.Xml.Linq;

namespace Microsoft.Maui.IntegrationTests;

[Category(Categories.Build)]
public class SimpleTemplateTest : BaseTemplateTests
{
	[Fact]
	// Parameters: short name, target framework, build config, use pack target, additionalDotNetNewParams, additionalDotNetBuildParams
	[Theory]
		[InlineData("maui", DotNetPrevious, "Debug", false, "", "")]
	[Theory]
		[InlineData("maui", DotNetPrevious, "Release", false, "", "")]
	[Theory]
		[InlineData("maui", DotNetCurrent, "Debug", false, "", "")]
	[Theory]
		[InlineData("maui", DotNetCurrent, "Release", false, "", "TrimMode=partial")]
	[Theory]
		[InlineData("maui", DotNetCurrent, "Debug", false, "--sample-content", "")]
	[Theory]
		[InlineData("maui", DotNetCurrent, "Release", false, "--sample-content", "TrimMode=partial")]
	[Theory]
		[InlineData("maui-blazor", DotNetPrevious, "Debug", false, "", "")]
	[Theory]
		[InlineData("maui-blazor", DotNetPrevious, "Release", false, "", "")]
	[Theory]
		[InlineData("maui-blazor", DotNetCurrent, "Debug", false, "", "")]
	[Theory]
		[InlineData("maui-blazor", DotNetCurrent, "Release", false, "", "TrimMode=partial")]
	[Theory]
		[InlineData("maui-blazor", DotNetCurrent, "Debug", false, "--Empty", "")]
	[Theory]
		[InlineData("maui-blazor", DotNetCurrent, "Release", false, "--Empty", "TrimMode=partial")]
	[Theory]
		[InlineData("mauilib", DotNetPrevious, "Debug", true, "", "")]
	[Theory]
		[InlineData("mauilib", DotNetPrevious, "Release", true, "", "")]
	[Theory]
		[InlineData("mauilib", DotNetCurrent, "Debug", true, "", "")]
	[Theory]
		[InlineData("mauilib", DotNetCurrent, "Release", true, "", "TrimMode=partial")]
	public void Build(string id, string framework, string config, bool shouldPack, string additionalDotNetNewParams, string additionalDotNetBuildParams)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework, additionalDotNetNewParams),
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
		Assert.True(DotnetInternal.Build(projectFile, config, target: target, properties: buildProps, msbuildWarningsAsErrors: true, warningsToIgnore: warningsToIgnore),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Fact]
	[Theory]
		[InlineData("maui", DotNetPrevious, "Debug")]
	public void InstallPackagesIntoUnsupportedTfmFails(string id, string framework, string config)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework),
			$"Unable to create template {id}. Check test output for errors.");

		FileUtilities.ReplaceInFile(projectFile,
			"$(MauiVersion)",
			MauiPackageVersion);

		Assert.False(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} built, but should not have. Check test output/attachments for why.");
	}

	[Fact]
	// with spaces
	[Theory]
		[InlineData("maui", "Project Space", "projectspace")]
	[Theory]
		[InlineData("maui-blazor", "Project Space", "projectspace")]
	[Theory]
		[InlineData("mauilib", "Project Space", "projectspace")]
	// with invalid characters
	[Theory]
		[InlineData("maui", "Project@Symbol", "projectsymbol")]
	[Theory]
		[InlineData("maui-blazor", "Project@Symbol", "projectsymbol")]
	[Theory]
		[InlineData("mauilib", "Project@Symbol", "projectsymbol")]
	public void BuildsWithSpecialCharacters(string id, string projectName, string expectedId)
	{
		var projectDir = Path.Combine(TestDirectory, projectName);
		var projectFile = Path.Combine(projectDir, $"{projectName}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, DotNetCurrent),
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

	[Fact]
	// Parameters: short name, target framework, build config, use pack target, additionalDotNetBuildParams
	[Theory]
		[InlineData("maui", DotNetPrevious, "Debug", false, "")]
	[Theory]
		[InlineData("maui", DotNetPrevious, "Release", false, "")]
	[Theory]
		[InlineData("maui", DotNetCurrent, "Debug", false, "")]
	[Theory]
		[InlineData("maui", DotNetCurrent, "Release", false, "TrimMode=partial")]
	[Theory]
		[InlineData("maui-blazor", DotNetPrevious, "Debug", false, "")]
	[Theory]
		[InlineData("maui-blazor", DotNetPrevious, "Release", false, "")]
	[Theory]
		[InlineData("maui-blazor", DotNetCurrent, "Debug", false, "")]
	[Theory]
		[InlineData("maui-blazor", DotNetCurrent, "Release", false, "TrimMode=partial")]
	[Theory]
		[InlineData("mauilib", DotNetPrevious, "Debug", true, "")]
	[Theory]
		[InlineData("mauilib", DotNetPrevious, "Release", true, "")]
	[Theory]
		[InlineData("mauilib", DotNetCurrent, "Debug", true, "")]
	[Theory]
		[InlineData("mauilib", DotNetCurrent, "Release", true, "TrimMode=partial")]
	public void BuildWithMauiVersion(string id, string framework, string config, bool shouldPack, string additionalDotNetBuildParams)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework),
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
		Assert.True(DotnetInternal.Build(projectFile, config, target: target, binlogPath: binlogDir, properties: buildProps),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

#if ENABLE_PREVIOUS_TFM_BUILDS
	[Fact]
	[Theory]
		[InlineData("maui", "Debug", false)]
	[Theory]
		[InlineData("maui", "Release", false)]
	[Theory]
		[InlineData("maui-blazor", "Debug", false)]
	[Theory]
		[InlineData("maui-blazor", "Release", false)]
	[Theory]
		[InlineData("mauilib", "Debug", true)]
	[Theory]
		[InlineData("mauilib", "Release", true)]
	public void PreviousDotNetCanUseLatestMaui(string id, string config, bool shouldPack)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, DotNetPrevious),
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

		EnableTizen(projectFile);
		File.WriteAllText(Path.Combine(projectDir, "Resources", "Images", ".DS_Store"), "Boom!");

		Assert.True(DotnetInternal.Build(projectFile, "Debug", properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	/// <summary>
	/// Tests the scenario where a .NET MAUI Library specifically uses UseMauiCore instead of UseMaui.
	/// </summary>
	[Fact]
	[Theory]
		[InlineData("mauilib", DotNetPrevious, "Debug")]
	[Theory]
		[InlineData("mauilib", DotNetPrevious, "Release")]
	[Theory]
		[InlineData("mauilib", DotNetCurrent, "Debug")]
	[Theory]
		[InlineData("mauilib", DotNetCurrent, "Release")]
	public void PackCoreLib(string id, string framework, string config)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework),
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

		Assert.True(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Fact]
	[Theory]
		[InlineData("maui", DotNetCurrent, "Debug")]
	[Theory]
		[InlineData("mauilib", DotNetCurrent, "Debug")]
	[Theory]
		[InlineData("maui-blazor", DotNetCurrent, "Debug")]
	public void BuildWithoutPackageReference(string id, string framework, string config)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework),
			$"Unable to create template {id}. Check test output for errors.");

		EnableTizen(projectFile);
		FileUtilities.ReplaceInFile(projectFile,
			"</Project>",
			"<PropertyGroup><SkipValidateMauiImplicitPackageReferences>true</SkipValidateMauiImplicitPackageReferences></PropertyGroup></Project>");
		FileUtilities.ReplaceInFile(projectFile,
			"<PackageReference Include=\"Microsoft.Maui.Controls\" Version=\"$(MauiVersion)\" />",
			"");

		Assert.True(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Fact]
	[Theory]
		[InlineData("maui", "Debug", "2.0", "2", "")]
	[Theory]
		[InlineData("maui", "Release", "2.0", "2", "TrimMode=partial")]
	[Theory]
		[InlineData("maui", "Release", "0.3", "3", "TrimMode=partial")]
	[Theory]
		[InlineData("maui-blazor", "Debug", "2.0", "2", "")]
	[Theory]
		[InlineData("maui-blazor", "Release", "2.0", "2", "TrimMode=partial")]
	[Theory]
		[InlineData("maui-blazor", "Release", "0.3", "3", "TrimMode=partial")]
	public void BuildWithDifferentVersionNumber(string id, string config, string display, string version, string additionalDotNetBuildParams)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir),
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

		Assert.True(DotnetInternal.Build(projectFile, config, properties: buildProps, msbuildWarningsAsErrors: true),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	// This test is super temporary and is just for the interim
	// while we productize the CollectionViewHandler2. Once we
	// ship it as the default, this test will fail and can be deleted.
	[Fact]
	[Theory]
		[InlineData("maui", DotNetCurrent, "", false)]
	[Theory]
		[InlineData("maui", DotNetCurrent, "--sample-content", true)]
	public void SampleShouldHaveHandler2Registered(string id, string framework, string additionalDotNetNewParams, bool shouldHaveHandler2)
	{
		var projectDir = TestDirectory;
		var programFile = Path.Combine(projectDir, "MauiProgram.cs");

		Assert.True(DotnetInternal.New(id, projectDir, framework, additionalDotNetNewParams),
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
}
