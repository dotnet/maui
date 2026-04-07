using System.Xml.Linq;

namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class SimpleTemplateTest : BaseTemplateTests
{
	const string AvaloniaBuildSkipReason = "Avalonia packages are not available on dotnet-public. See https://github.com/dotnet/maui/pull/35950";

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
	[InlineData("maui", DotNetCurrent, "Debug", false, "--with-avalonia", "", Skip = AvaloniaBuildSkipReason)]
	[InlineData("maui", DotNetCurrent, "Release", false, "--with-avalonia", "TrimMode=partial", Skip = AvaloniaBuildSkipReason)]
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
		var usesAvalonia = additionalDotNetNewParams.Contains("--with-avalonia", StringComparison.Ordinal);
		var newParams = usesAvalonia ? $"{additionalDotNetNewParams} --no-restore" : additionalDotNetNewParams;

		Assert.True(DotnetInternal.New(id, projectDir, framework, newParams, output: _output),
			$"Unable to create template {id}. Check test output for errors.");


		if (shouldPack)
			FileUtilities.ReplaceInFile(projectFile,
				"</Project>",
				"<PropertyGroup><Version>1.0.0-preview.1</Version></PropertyGroup></Project>");

		var buildProps = BuildProps;

		if (usesAvalonia)
		{
			buildProps.RemoveAll(p => p.StartsWith("RestoreConfigFile=", StringComparison.Ordinal));
			buildProps.Add($"RestoreConfigFile={CreateAvaloniaNuGetConfig(projectDir)}");
		}

		if (additionalDotNetBuildParams is not "" and not null)
		{
			additionalDotNetBuildParams.Split(" ").ToList().ForEach(p => buildProps.Add(p));
		}

		string target = shouldPack ? "Pack" : "";
		Assert.True(DotnetInternal.Build(projectFile, config, target: target, properties: buildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	private string CreateAvaloniaNuGetConfig(string projectDir)
	{
		var config = XDocument.Load(TestNuGetConfig);
		var packageSources = config.Root!.Element("packageSources")!;
		const string nugetOrg = "nuget.org";

		packageSources.Add(
			new XElement("add",
				new XAttribute("key", nugetOrg),
				new XAttribute("value", "https://api.nuget.org/v3/index.json"),
				new XAttribute("protocolVersion", "3")));

		var sourceMapping = new XElement("packageSourceMapping");
		foreach (var source in packageSources.Elements("add"))
		{
			var key = source.Attribute("key")!.Value;
			var patterns = key == nugetOrg ? new[] { "Avalonia*", "MicroCom.*" } : new[] { "*" };
			sourceMapping.Add(
				new XElement("packageSource",
					new XAttribute("key", key),
					patterns.Select(pattern =>
						new XElement("package",
							new XAttribute("pattern", pattern)))));
		}

		config.Root.Add(sourceMapping);
		var path = Path.Combine(projectDir, "NuGet.config");
		config.Save(path);
		return path;
	}

	[Theory]
	[InlineData("maui")]
	[InlineData("maui-blazor")]
	[InlineData("mauilib")]
	public void NewProjectIncludesGitIgnore(string id)
	{
		SetTestIdentifier(id);
		var projectDir = TestDirectory;

		Assert.True(DotnetInternal.New(id, projectDir, DotNetCurrent, output: _output),
			$"Unable to create template {id}. Check test output for errors.");

		AssertIncludesRootGitIgnore(projectDir);
	}

	[Theory]
	[InlineData(DotNetCurrent, "Debug", "")]
	[InlineData(DotNetCurrent, "Release", "TrimMode=partial")]
	public void BuildMauiCSharpUI(string framework, string config, string additionalDotNetBuildParams)
	{
		SetTestIdentifier(framework, config, additionalDotNetBuildParams);
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New("maui", projectDir, framework, "--ui csharp --no-restore", output: _output),
			"Unable to create template maui with --ui csharp. Check test output for errors.");

		var mainPageFile = Path.Combine(projectDir, "MainPage.cs");
		var mainPageContent = File.ReadAllText(mainPageFile);
		var mauiProgramContent = File.ReadAllText(Path.Combine(projectDir, "MauiProgram.cs"));
		Assert.True(File.Exists(Path.Combine(projectDir, "App.cs")));
		Assert.True(File.Exists(Path.Combine(projectDir, "AppShell.cs")));
		Assert.True(File.Exists(mainPageFile));
		Assert.False(File.Exists(Path.Combine(projectDir, "App.xaml")));
		Assert.False(File.Exists(Path.Combine(projectDir, "App.xaml.cs")));
		Assert.False(File.Exists(Path.Combine(projectDir, "AppShell.xaml")));
		Assert.False(File.Exists(Path.Combine(projectDir, "AppShell.xaml.cs")));
		Assert.False(File.Exists(Path.Combine(projectDir, "MainPage.xaml")));
		Assert.False(File.Exists(Path.Combine(projectDir, "MainPage.xaml.cs")));

		AssertContains("using CommunityToolkit.Maui.Markup;", mainPageContent);
		Assert.True(mainPageContent.Contains(".CenterHorizontal()", StringComparison.Ordinal),
			"Expected generated markup UI to use CommunityToolkit layout extensions.");
		Assert.True(mainPageContent.Contains(".TextCenter()", StringComparison.Ordinal) || mainPageContent.Contains(".TextCenterHorizontal()", StringComparison.Ordinal),
			"Expected generated markup UI to use CommunityToolkit text-centering extensions.");
		Assert.True(mainPageContent.Contains(".Fill()", StringComparison.Ordinal) || mainPageContent.Contains(".FillHorizontal()", StringComparison.Ordinal),
			"Expected generated markup UI to use CommunityToolkit fill extensions.");
		AssertContains(".UseMauiCommunityToolkitMarkup()", mauiProgramContent);

		var projectDoc = XDocument.Load(projectFile);
		Assert.Contains(projectDoc.Descendants("PackageReference"),
			packageReference => packageReference.Attribute("Include")?.Value == "CommunityToolkit.Maui.Markup");

		var buildProps = BuildProps;

		if (additionalDotNetBuildParams is not "" and not null)
		{
			additionalDotNetBuildParams.Split(" ").ToList().ForEach(p => buildProps.Add(p));
		}

		Assert.True(DotnetInternal.Build(projectFile, config, properties: buildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Theory]
	[InlineData("maui", DotNetPrevious, "Debug")]
	public void InstallPackagesIntoUnsupportedTfmFails(string id, string framework, string config)
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New(id, projectDir, framework, output: _output),
			$"Unable to create template {id}. Check test output for errors.");

		FileUtilities.ReplaceInFile(projectFile,
			"$(MauiVersion)",
			MauiPackageVersion);

		Assert.False(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Project {Path.GetFileName(projectFile)} built, but should not have. Check test output/attachments for why.");
	}

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

		Assert.True(DotnetInternal.New(id, projectDir, DotNetCurrent, output: _output),
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

		Assert.True(DotnetInternal.Build(projectFile, "Debug", properties: BuildProps, msbuildWarningsAsErrors: true, output: _output),
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

		Assert.True(DotnetInternal.New(id, projectDir, framework, output: _output),
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
		Assert.True(DotnetInternal.Build(projectFile, config, target: target, binlogPath: binlogDir, properties: buildProps, output: _output),
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

		Assert.True(DotnetInternal.New(id, projectDir, DotNetPrevious, output: _output),
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
		Assert.True(DotnetInternal.Build(projectFile, config, target: target, properties: BuildProps, output: _output),
			$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
	}
#endif

	[Fact]
	public void BuildHandlesBadFilesInImages()
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New("maui", projectDir, DotNetCurrent, output: _output),
			$"Unable to create template maui. Check test output for errors.");

		File.WriteAllText(Path.Combine(projectDir, "Resources", "Images", ".DS_Store"), "Boom!");

		Assert.True(DotnetInternal.Build(projectFile, "Debug", properties: BuildProps, msbuildWarningsAsErrors: true, output: _output),
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

		Assert.True(DotnetInternal.New(id, projectDir, framework, output: _output),
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

		Assert.True(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true, output: _output),
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

		Assert.True(DotnetInternal.New(id, projectDir, framework, output: _output),
			$"Unable to create template {id}. Check test output for errors.");

		FileUtilities.ReplaceInFile(projectFile,
			"</Project>",
			"<PropertyGroup><SkipValidateMauiImplicitPackageReferences>true</SkipValidateMauiImplicitPackageReferences></PropertyGroup></Project>");
		FileUtilities.ReplaceInFile(projectFile,
			"<PackageReference Include=\"Microsoft.Maui.Controls\" Version=\"$(MauiVersion)\" />",
			"");

		Assert.True(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true, output: _output),
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

		Assert.True(DotnetInternal.New(id, projectDir, output: _output),
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

		Assert.True(DotnetInternal.Build(projectFile, config, properties: buildProps, msbuildWarningsAsErrors: true, output: _output),
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

		Assert.True(DotnetInternal.New("maui-aspire-servicedefaults", projectDir, additionalDotNetNewParams: $"-n \"{projectName}\"", output: _output),
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

		// Verify the project file contains required properties
		var projectContent = File.ReadAllText(expectedProjectFile);
		Assert.True(projectContent.Contains("<IsAspireSharedProject>true</IsAspireSharedProject>", StringComparison.Ordinal),
			"Project file should contain Aspire-specific properties.");
		Assert.True(projectContent.Contains("<UseMauiCore>true</UseMauiCore>", StringComparison.Ordinal),
			"Project file should contain UseMauiCore property.");

		// Verify the project actually builds
		Assert.True(DotnetInternal.Build(expectedProjectFile, "Debug", properties: BuildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Project {Path.GetFileName(expectedProjectFile)} failed to build. Check test output/attachments for errors.");
	}

	[Fact]
	public void WithAvaloniaAddsHandlersAndDesktopHead()
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New("maui", projectDir, DotNetCurrent, "--with-avalonia --no-restore", output: _output),
			"Unable to create template maui with --with-avalonia. Check test output for errors.");

		var csproj = File.ReadAllText(projectFile);
		// The standard (non-platform) TFM is added as the Avalonia desktop head.
		AssertContains($"<TargetFrameworks>{DotNetCurrent};$(TargetFrameworks)</TargetFrameworks>", csproj);
		// Handlers reference is added for all heads; the Desktop package only targets the desktop head.
		AssertContains("Include=\"Avalonia.Controls.Maui\"", csproj);
		AssertContains("Include=\"Avalonia.Controls.Maui.Desktop\"", csproj);
		AssertContains($"Condition=\"'$(TargetFramework)' == '{DotNetCurrent}'\"", csproj);

		var mauiProgram = File.ReadAllText(Path.Combine(projectDir, "MauiProgram.cs"));
		AssertContains("CreateMauiApp(bool useSingleViewLifetime = false)", mauiProgram);
		// Desktop renders with the Avalonia app lifetime; the platform heads embed Avalonia.
		AssertContains(".UseAvaloniaApp(useSingleViewLifetime)", mauiProgram);
		AssertContains(".UseAvaloniaEmbedding<AvaloniaApp>()", mauiProgram);
	}

	[Fact]
	public void WithoutAvaloniaHasNoAvaloniaContent()
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New("maui", projectDir, DotNetCurrent, "--no-restore", output: _output),
			"Unable to create template maui. Check test output for errors.");

		var csproj = File.ReadAllText(projectFile);
		AssertDoesNotContain("Avalonia.Controls.Maui", csproj);

		var mauiProgram = File.ReadAllText(Path.Combine(projectDir, "MauiProgram.cs"));
		AssertDoesNotContain("UseAvalonia", mauiProgram);
		AssertContains("public static MauiApp CreateMauiApp()", mauiProgram);
	}

	[Fact]
	public void WithAvaloniaIsIgnoredWhenSampleContentIncluded()
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		// --with-avalonia is gated on the blank app: combining it with sample content must not wire Avalonia in.
		Assert.True(DotnetInternal.New("maui", projectDir, DotNetCurrent, "--with-avalonia --sample-content --no-restore", output: _output),
			"Unable to create template maui with --with-avalonia --sample-content. Check test output for errors.");

		var csproj = File.ReadAllText(projectFile);
		AssertDoesNotContain("Avalonia.Controls.Maui", csproj);

		var mauiProgram = File.ReadAllText(Path.Combine(projectDir, "MauiProgram.cs"));
		AssertDoesNotContain("UseAvalonia", mauiProgram);
	}
}
