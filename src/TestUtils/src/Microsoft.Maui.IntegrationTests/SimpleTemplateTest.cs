using System.IO.Compression;
using System.Security.Cryptography;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class SimpleTemplateTest : BaseTemplateTests
{
	public SimpleTemplateTest(IntegrationTestFixture fixture, ITestOutputHelper output) : base(fixture, output) { }

	[Theory]
	// Parameters: short name, target framework, build config, use pack target, additionalDotNetNewParams, additionalDotNetBuildParams
	[InlineData("maui", DotNetPrevious, "Debug", false, "", "")]
	[InlineData("maui", DotNetPrevious, "Release", false, "", "")]
	[InlineData("maui", DotNetCurrent, "Debug", false, "", "")]
	[InlineData("maui", DotNetCurrent, "Release", false, "", "TrimMode=partial")]
	[InlineData("maui", DotNetCurrent, "Debug", false, "--sample-content", "")]
	[InlineData("maui", DotNetCurrent, "Release", false, "--sample-content", "TrimMode=partial")]
	//Debug not ready yet
	//[InlineData("maui", DotNetCurrent, "Debug", false, "--sample-content", "UseMonoRuntime=false")]
	[InlineData("maui", DotNetCurrent, "Release", false, "--sample-content", "UseMonoRuntime=false EnablePreviewFeatures=true")]
	[InlineData("maui", DotNetCurrent, "Debug", false, "--with-avalonia", "", Skip = AvaloniaBuildSkipReason)]
	[InlineData("maui", DotNetCurrent, "Release", false, "--with-avalonia", "TrimMode=partial", Skip = AvaloniaBuildSkipReason)]
	[InlineData("maui-blazor", DotNetPrevious, "Debug", false, "", "")]
	[InlineData("maui-blazor", DotNetPrevious, "Release", false, "", "")]
	[InlineData("maui-blazor", DotNetCurrent, "Debug", false, "", "")]
	[InlineData("maui-blazor", DotNetCurrent, "Release", false, "", "TrimMode=partial")]
	[InlineData("maui-blazor", DotNetCurrent, "Debug", false, "--empty", "")]
	[InlineData("maui-blazor", DotNetCurrent, "Release", false, "--empty", "TrimMode=partial")]
	[InlineData("mauilib", DotNetPrevious, "Debug", true, "", "")]
	[InlineData("mauilib", DotNetPrevious, "Release", true, "", "")]
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

	[Fact]
	public void SideBySideTemplateHelpIncludesSampleContentOnce()
	{
		var legacyTemplateDir = Path.Combine(TestDirectory, "legacy-template");
		var legacyTemplateConfigDir = Path.Combine(legacyTemplateDir, ".template.config");
		var customHive = Path.Combine(TestDirectory, "template-hive");
		Directory.CreateDirectory(legacyTemplateConfigDir);

		File.WriteAllText(Path.Combine(legacyTemplateConfigDir, "template.json"), """
			{
			  "$schema": "http://json.schemastore.org/template",
			  "author": "Microsoft",
			  "identity": "Microsoft.Maui.MauiApp.CSharp.10.0",
			  "groupIdentity": "Microsoft.Maui.App",
			  "precedence": "10",
			  "name": ".NET MAUI App",
			  "shortName": "maui",
			  "sourceName": "MauiApp.1",
			  "symbols": {
			    "IncludeSampleContent": {
			      "type": "parameter",
			      "datatype": "bool",
			      "defaultValue": "false",
			      "displayName": "_Include sample content",
			      "description": "Configures whether to add sample pages and functionality to demonstrate basic usage patterns."
			    }
			  }
			}
			""");
		File.WriteAllText(Path.Combine(legacyTemplateConfigDir, "dotnetcli.host.json"), """
			{
			  "$schema": "https://json.schemastore.org/dotnetcli.host",
			  "symbolInfo": {
			    "IncludeSampleContent": {
			      "longName": "sample-content",
			      "shortName": "sc"
			    }
			  }
			}
			""");
		File.WriteAllText(Path.Combine(legacyTemplateDir, "MauiApp.1.csproj"), "<Project />");

		var templatePack = GetTemplatePackPath();
		Assert.True(File.Exists(templatePack), $"Template pack '{templatePack}' does not exist.");

		var installOutput = DotnetInternal.RunForOutput(
			"new",
			$"--debug:custom-hive \"{customHive}\" install \"{legacyTemplateDir}\" \"{templatePack}\"",
			out int installExitCode,
			output: _output);
		_output.WriteLine(installOutput);
		Assert.True(installExitCode == 0, "Unable to install side-by-side template packs.");

		var helpOutput = DotnetInternal.RunForOutput(
			"new",
			$"--debug:custom-hive \"{customHive}\" maui --help",
			out int helpExitCode,
			output: _output);
		_output.WriteLine(helpOutput);
		Assert.True(helpExitCode == 0, "Unable to show side-by-side template help.");
		AssertContains("--ui", helpOutput);
		AssertContains("in the XAML experience.", helpOutput);
		var sampleContentOptionCount = 0;
		using var helpReader = new StringReader(helpOutput);
		while (helpReader.ReadLine() is { } line)
		{
			if (line.Contains("--sample-content", StringComparison.Ordinal))
				sampleContentOptionCount++;
		}
		Assert.Equal(1, sampleContentOptionCount);

		foreach (var (options, expectAvalonia) in new[]
		{
			("--ui csharp --sample-content --no-restore", false),
			("--ui csharp --sample-content --with-avalonia --no-restore", true),
		})
		{
			var projectDir = Path.Combine(TestDirectory, expectAvalonia ? "csharp-sample-avalonia" : "csharp-sample");
			var commandOutput = DotnetInternal.RunForOutput(
				"new",
				$"--debug:custom-hive \"{customHive}\" maui -o \"{projectDir}\" -f {DotNetCurrent} {options}",
				out int exitCode,
				output: _output);
			Assert.True(exitCode == 0, $"Unable to create side-by-side template with '{options}'.");
			AssertContains("Warning: The sample content option was not applied.", commandOutput);
			AssertDoesNotContain("Warning: The Avalonia option was not applied.", commandOutput);
			Assert.False(Directory.Exists(Path.Combine(projectDir, "Pages")));

			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");
			var projectContent = File.ReadAllText(projectFile);
			Assert.Equal(expectAvalonia, projectContent.Contains("Avalonia.Controls.Maui", StringComparison.Ordinal));
		}
	}

	[Theory]
	[InlineData("", "dotnetcli", false, false, false)]
	[InlineData("--ui csharp", "dotnetcli", false, true, false)]
	[InlineData("--sample-content", "dotnetcli", true, false, false)]
	[InlineData("--IncludeSampleContentIde true", "vs", true, false, false)]
	[InlineData("--sample-content --with-avalonia", "dotnetcli", true, false, false)]
	[InlineData("--ui csharp --sample-content", "dotnetcli", false, true, false)]
	[InlineData("--ui csharp --sample-content --with-avalonia", "dotnetcli", false, true, true)]
	public void GeneratedMauiOptionsRespectContentBoundaries(string options, string host, bool sample, bool csharp, bool avalonia)
	{
		SetTestIdentifier(options, host);
		var customHive = Path.Combine(TestDirectory, "template-hive");
		var projectDir = Path.Combine(TestDirectory, "GeneratedApp");
		var templatePack = GetTemplatePackPath();
		Assert.True(File.Exists(templatePack), $"Template pack '{templatePack}' does not exist.");
		var installSource = templatePack;
		if (host == "vs")
		{
			// The CLI fixes its host identifier. Change only that input in a disposable copy
			// to exercise the IDE option expression with the same template engine.
			// This does not test the IDE host API.
			installSource = Path.Combine(TestDirectory, "ide-template");
			ZipFile.ExtractToDirectory(templatePack, installSource);
			var configFile = Path.Combine(installSource, "content", "templates", "maui-mobile", ".template.config", "template.json");
			var config = JsonNode.Parse(File.ReadAllText(configFile))!;
			var symbols = config["symbols"]!.AsObject();
			Assert.Equal("bind", symbols["HostIdentifier"]!["type"]!.GetValue<string>());
			Assert.Equal("HostIdentifier", symbols["HostIdentifier"]!["binding"]!.GetValue<string>());
			symbols["HostIdentifier"] = new JsonObject
			{
				["type"] = "parameter",
				["datatype"] = "string",
				["defaultValue"] = host,
			};
			File.WriteAllText(configFile, config.ToJsonString(new JsonSerializerOptions { WriteIndented = true }));
			_output.WriteLine("IDE option-branch simulation through the CLI engine with HostIdentifier=vs. IDE host-API and Visual Studio execution remain unverified.");
		}
		Assert.True(DotnetInternal.Run("new",
			$"--debug:custom-hive \"{customHive}\" install \"{installSource}\"", output: _output),
			"Unable to install the template pack in the isolated hive.");

		var hostOptions = host == "vs" ? "--HostIdentifier vs" : "";
		var commandOutput = DotnetInternal.RunForOutput("new",
			$"--debug:custom-hive \"{customHive}\" maui -o \"{projectDir}\" -f {DotNetCurrent} {options} {hostOptions} --no-restore",
			out var exitCode, timeoutInSeconds: 300, output: _output);
		_output.WriteLine(commandOutput);
		Assert.Equal(0, exitCode);
		Assert.Equal(csharp && options.Contains("--sample-content", StringComparison.Ordinal),
			commandOutput.Contains("Warning: The sample content option was not applied.", StringComparison.Ordinal));
		Assert.Equal(sample && options.Contains("--with-avalonia", StringComparison.Ordinal),
			commandOutput.Contains("Warning: The Avalonia option was not applied.", StringComparison.Ordinal));

		var project = XDocument.Load(Path.Combine(projectDir, "GeneratedApp.csproj"));
		Assert.Equal(avalonia, project.Descendants("PackageReference").Any(p =>
			((string?)p.Attribute("Include"))?.StartsWith("Avalonia.", StringComparison.Ordinal) == true));
		Assert.Equal(sample, project.Descendants("EnablePreviewFeatures").Any(p => p.Value == "true"));
		Assert.Equal(sample, project.Descendants("MauiEnableXamlCBindingWithSourceCompilation").Any(p => p.Value == "true"));
		Assert.Empty(project.Descendants("PublishAot"));
		Assert.Empty(project.Descendants("PublishTrimmed"));
		Assert.Empty(project.Descendants("NoWarn"));
		var splash = Assert.Single(project.Descendants("MauiSplashScreen"));
		Assert.Equal("Resources/Splash/splash.svg", ((string?)splash.Attribute("Include"))?.Replace('\\', '/'));
		Assert.Equal("128,128", (string?)splash.Attribute("BaseSize"));
		Assert.Equal(sample ? "#F2F2F2" : "#512BD4", (string?)splash.Attribute("Color"));
		Assert.Equal(sample ? "#0D0D0D" : null, (string?)splash.Attribute("TintColor"));
		Assert.Equal(sample ? "#17171a" : null, (string?)splash.Attribute("DarkColor"));
		Assert.Equal(sample ? "#C3C3C3" : null, (string?)splash.Attribute("DarkTintColor"));
		var mauiProgram = File.ReadAllText(Path.Combine(projectDir, "MauiProgram.cs"));
		AssertContains("#if DEBUG", mauiProgram);
		AssertContains("builder.Logging.AddDebug();", mauiProgram);
		Assert.Equal(1, mauiProgram.Split("AddDebug(", StringSplitOptions.None).Length - 1);

		foreach (var directory in new[] { "Data", "Models", "PageModels", "Pages", "Services", "Utilities" })
			Assert.Equal(sample, Directory.Exists(Path.Combine(projectDir, directory)));
		if (!sample)
		{
			Assert.False(Directory.Exists(Path.Combine(projectDir, "Converter")));
			Assert.False(Directory.Exists(Path.Combine(projectDir, "Messages")));
		}
		foreach (var file in new[]
		{
			"GlobalUsings.cs", "GlobalXmlns.cs", "Resources/Raw/SeedData.json",
			"Resources/Styles/AppStyles.xaml",
		})
			Assert.Equal(sample, File.Exists(Path.Combine(projectDir, file)));

		Assert.Equal(csharp, File.Exists(Path.Combine(projectDir, "MainPage.cs")));
		Assert.Equal(!sample && !csharp, File.Exists(Path.Combine(projectDir, "MainPage.xaml")));
		Assert.Equal(!sample && !csharp, File.Exists(Path.Combine(projectDir, "MainPage.xaml.cs")));
		var app = File.ReadAllText(Path.Combine(projectDir, csharp ? "App.cs" : "App.xaml.cs"));
		Assert.Equal(sample, app.Contains("StatusBarTheme", StringComparison.Ordinal));
		Assert.Equal(sample, app.Contains("RequestedThemeChanged +=", StringComparison.Ordinal));
		var shell = File.ReadAllText(Path.Combine(projectDir, csharp ? "AppShell.cs" : "AppShell.xaml.cs"));
		Assert.Equal(sample, shell.Contains("ThemeSegmentedControl", StringComparison.Ordinal));
		Assert.Equal(sample, shell.Contains("UpdateBackButtonAccessibility", StringComparison.Ordinal));
		AssertSampleProjectMetadata(project, templatePack, projectDir, sample);

		var botFiles = Directory.GetFiles(projectDir, "dotnet_bot*", SearchOption.AllDirectories);
		if (sample)
		{
			Assert.Empty(botFiles);
			Assert.DoesNotContain(project.Descendants("MauiImage"), image =>
				image.Attributes().Any(attribute => attribute.Value.Contains("dotnet_bot", StringComparison.Ordinal)));
			var xmlns = File.ReadAllText(Path.Combine(projectDir, "GlobalXmlns.cs"));
			foreach (var mappedNamespace in new[] { "GeneratedApp.Pages", "GeneratedApp.Pages.Controls", "GeneratedApp.PageModels", "GeneratedApp.Models", "Fonts" })
				AssertContains($"\"{mappedNamespace}\"", xmlns);
			AssertDoesNotContain("MauiApp._1", xmlns);
			AssertDoesNotContain("Syncfusion", xmlns);
			AssertContains("#if ANDROID || WINDOWS", shell);
			var dashboard = File.ReadAllText(Path.Combine(projectDir, "Pages", "MainPage.xaml"));
			AssertContains("x:DataType=\"MainPageModel\"", dashboard);
			AssertContains("IsEnabled=\"{!IsBusy}\"", dashboard);
			foreach (var xaml in Directory.GetFiles(projectDir, "*.xaml", SearchOption.AllDirectories))
			{
				AssertDoesNotContain("{OnIdiom", File.ReadAllText(xaml));
				AssertDoesNotContain("{OnPlatform", File.ReadAllText(xaml));
			}
			var manageMeta = File.ReadAllText(Path.Combine(projectDir, "Pages", "ManageMetaPage.xaml"));
			AssertDoesNotContain("TextValidationBehavior", manageMeta);
			Assert.Equal(2, Regex.Matches(manageMeta, @"\bUnfocused=""[^""]+""").Count);
			var manageMetaCode = File.ReadAllText(Path.Combine(projectDir, "Pages", "ManageMetaPage.xaml.cs"));
			AssertContains("[GeneratedRegex(", manageMetaCode);
		}
		else
		{
			var bot = Assert.Single(botFiles);
			Assert.Equal(Path.Combine(projectDir, "Resources", "Images", "dotnet_bot.png"), bot);
			Assert.Equal("dac6f5c17bc85a0e9829206705719ff65197035f7356621724c790931b4eb026",
				Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(bot))).ToLowerInvariant());
			var image = Assert.Single(project.Descendants("MauiImage"), image =>
				((string?)image.Attribute("Update"))?.Replace('\\', '/') == "Resources/Images/dotnet_bot.png");
			Assert.Equal("true", (string?)image.Attribute("Resize"));
			Assert.Equal("190,185", (string?)image.Attribute("BaseSize"));
			Assert.DoesNotContain(project.Descendants("MauiImage"), image =>
				((string?)image.Attribute("Include"))?.Contains("dotnet_bot", StringComparison.Ordinal) == true);
			Assert.DoesNotContain(project.Descendants("MauiIcon"), icon => icon.Attribute("MonochromeFile") is not null);

			var page = File.ReadAllText(Path.Combine(projectDir, csharp ? "MainPage.cs" : "MainPage.xaml"));
			AssertContains("dotnet_bot.png", page);
			AssertContains("Two dot net bots with a rocket marked eleven.", page);
			AssertContains(csharp ? "HeightRequest = 185" : "HeightRequest=\"185\"", page);
			AssertContains(csharp ? "MaximumWidthRequest = 190" : "MaximumWidthRequest=\"190\"", page);
			AssertContains(csharp ? "Aspect = Aspect.AspectFit" : "Aspect=\"AspectFit\"", page);
			AssertContains(csharp ? "HorizontalOptions = LayoutOptions.Center" : "HorizontalOptions=\"Center\"", page);
			AssertContains("Click me", page);
			AssertContains("OnCounterClicked", page);
			var counter = csharp ? page : File.ReadAllText(Path.Combine(projectDir, "MainPage.xaml.cs"));
			AssertContains(" time\"", counter);
			AssertContains(" times\"", counter);
			AssertContains("SemanticScreenReader.Announce(", counter);
		}
	}

	string GetTemplatePackPath() => Path.Combine(
		TestEnvironment.GetMauiDirectory(), ".dotnet", "template-packs",
		$"Microsoft.Maui.Templates.{DotNetCurrent.Split('.')[0]}.{MauiPackageVersion}.nupkg");

	[Theory]
	[InlineData("Microsoft.Data.Sqlite.Core", "MicrosoftDataSqliteCorePackageVersion")]
	[InlineData("SQLitePCLRaw.bundle_e_sqlite3", "SQLitePCLRawBundleESqlite3PackageVersion")]
	[InlineData("CommunityToolkit.Maui", "CommunityToolkitMauiPackageVersion")]
	[InlineData("CommunityToolkit.Mvvm", "CommunityToolkitMvvmPackageVersion")]
	[InlineData("Syncfusion.Maui.Toolkit", "SyncfusionMauiToolkitPackageVersion")]
	public void SampleDependencyMatchesComponentInventory(string package, string versionProperty)
	{
		var versions = XDocument.Load(Path.Combine(TestEnvironment.GetMauiDirectory(), "eng", "Versions.props"));
		var inventoryVersion = Assert.Single(versions.Descendants(versionProperty)).Value;
		using var archive = ZipFile.OpenRead(GetTemplatePackPath());
		var projectEntry = Assert.Single(archive.Entries, entry =>
			entry.FullName.EndsWith("/maui-mobile/MauiApp.1.csproj", StringComparison.Ordinal));
		using var source = projectEntry.Open();
		var project = XDocument.Load(source);
		var reference = Assert.Single(project.Descendants("PackageReference"),
			item => (string?)item.Attribute("Include") == package);

		Assert.Equal(inventoryVersion, (string?)reference.Attribute("Version"));
	}

	static void AssertSampleProjectMetadata(XDocument project, string templatePack, string projectDir, bool sample)
	{
		// Compare versions with the installed pack rather than a second dependency-version baseline.
		using var archive = ZipFile.OpenRead(templatePack);
		var projectEntry = Assert.Single(archive.Entries, entry =>
			entry.FullName.EndsWith("/maui-mobile/MauiApp.1.csproj", StringComparison.Ordinal));
		using var source = projectEntry.Open();
		var templateProject = XDocument.Load(source);
		foreach (var id in new[]
		{
			"CommunityToolkit.Mvvm", "CommunityToolkit.Maui", "Microsoft.Data.Sqlite.Core",
			"SQLitePCLRaw.bundle_e_sqlite3", "Syncfusion.Maui.Toolkit",
		})
		{
			var references = project.Descendants("PackageReference").Where(p => (string?)p.Attribute("Include") == id).ToArray();
			if (sample)
			{
				var expected = templateProject.Descendants("PackageReference").Single(p => (string?)p.Attribute("Include") == id);
				Assert.Equal((string?)expected.Attribute("Version"), (string?)Assert.Single(references).Attribute("Version"));
			}
			else
				Assert.Empty(references);
		}

		var sampleIcon = templateProject.Descendants("MauiIcon").Single(icon => icon.Attribute("MonochromeFile") is not null);
		var monochrome = sampleIcon.Attribute("MonochromeFile")!.Value;
		var generatedIcon = Assert.Single(project.Descendants("MauiIcon"));
		if (sample)
		{
			Assert.Equal(monochrome, (string?)generatedIcon.Attribute("MonochromeFile"));
			Assert.True(File.Exists(Path.Combine(projectDir, monochrome.Replace('\\', '/'))));
		}
		else if (monochrome != (string?)sampleIcon.Attribute("ForegroundFile"))
			Assert.False(File.Exists(Path.Combine(projectDir, monochrome.Replace('\\', '/'))),
				"A sample-only monochrome asset leaked into the default app.");
	}

	[Theory]
	[InlineData(DotNetCurrent, "Debug", "", "")]
	[InlineData(DotNetCurrent, "Release", "", "TrimMode=partial")]
	public void BuildMauiCSharpUI(string framework, string config, string additionalDotNetNewParams, string additionalDotNetBuildParams)
	{
		SetTestIdentifier(framework, config, additionalDotNetNewParams, additionalDotNetBuildParams);
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		var dotnetNewParams = $"--ui csharp --no-restore {additionalDotNetNewParams}".TrimEnd();
		Assert.True(DotnetInternal.New("maui", projectDir, framework, dotnetNewParams, output: _output),
			"Unable to create template maui with --ui csharp. Check test output for errors.");

		var mainPageFile = Path.Combine(projectDir, "MainPage.cs");
		var appFile = Path.Combine(projectDir, "App.cs");
		var appXamlFile = Path.Combine(projectDir, "App.xaml");
		var appShellFile = Path.Combine(projectDir, "AppShell.cs");
		Assert.True(File.Exists(appFile));
		Assert.True(File.Exists(appShellFile));
		Assert.True(File.Exists(mainPageFile));
		Assert.True(File.Exists(appXamlFile));
		Assert.True(File.Exists(Path.Combine(projectDir, "Resources", "Images", "dotnet_bot.png")));
		Assert.False(File.Exists(Path.Combine(projectDir, "App.xaml.cs")));
		Assert.False(File.Exists(Path.Combine(projectDir, "AppShell.xaml")));
		Assert.False(File.Exists(Path.Combine(projectDir, "AppShell.xaml.cs")));
		Assert.False(File.Exists(Path.Combine(projectDir, "MainPage.xaml")));
		Assert.False(File.Exists(Path.Combine(projectDir, "MainPage.xaml.cs")));
		Assert.False(File.Exists(Path.Combine(projectDir, "Resources", "Styles", "AppStyles.xaml")));
		Assert.False(Directory.Exists(Path.Combine(projectDir, "Pages")));

		var mainPageContent = File.ReadAllText(mainPageFile);
		var appContent = File.ReadAllText(appFile);
		var appShellContent = File.ReadAllText(appShellFile);
		var appXamlContent = File.ReadAllText(appXamlFile);
		var mauiProgramContent = File.ReadAllText(Path.Combine(projectDir, "MauiProgram.cs"));
		var projectContent = File.ReadAllText(projectFile);
		AssertContains("Margin = new Thickness(0, 20, 0, 0)", mainPageContent);
		AssertContains("SemanticProperties.SetDescription(logo, \"Two dot net bots with a rocket marked eleven.\")", mainPageContent);
		AssertContains("SetDynamicResource(VisualElement.StyleProperty, \"Headline\")", mainPageContent);
		AssertContains("SetDynamicResource(VisualElement.StyleProperty, \"SubHeadline\")", mainPageContent);
		AssertContains("MaximumWidthRequest = 190", mainPageContent);
		AssertDoesNotContain("FontSize = 18", mainPageContent);
		AssertDoesNotContain("FontAttributes = FontAttributes.Bold", mainPageContent);
		AssertContains("HorizontalOptions = LayoutOptions.Fill", mainPageContent);
		AssertContains("InitializeComponent();", appContent);
		AssertContains($"Title = \"{Path.GetFileName(projectDir)}\";", appShellContent);
		AssertContains("Resources/Styles/Colors.xaml", appXamlContent);
		AssertContains("Resources/Styles/Styles.xaml", appXamlContent);
		AssertDoesNotContain("CommunityToolkit.Maui", projectContent);
		AssertDoesNotContain("CommunityToolkit.Mvvm", projectContent);
		AssertDoesNotContain("Syncfusion.Maui.Toolkit", projectContent);
		AssertDoesNotContain("UseMauiCommunityToolkit", mauiProgramContent);
		AssertDoesNotContain("ConfigureSyncfusionToolkit", mauiProgramContent);

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
	[InlineData("maui", DotNetPrevious, "Debug", false, "")]
	[InlineData("maui", DotNetPrevious, "Release", false, "")]
	[InlineData("maui", DotNetCurrent, "Debug", false, "")]
	[InlineData("maui", DotNetCurrent, "Release", false, "TrimMode=partial")]
	[InlineData("maui-blazor", DotNetPrevious, "Debug", false, "")]
	[InlineData("maui-blazor", DotNetPrevious, "Release", false, "")]
	[InlineData("maui-blazor", DotNetCurrent, "Debug", false, "")]
	[InlineData("maui-blazor", DotNetCurrent, "Release", false, "TrimMode=partial")]
	[InlineData("mauilib", DotNetPrevious, "Debug", true, "")]
	[InlineData("mauilib", DotNetPrevious, "Release", true, "")]
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
	[InlineData("mauilib", DotNetPrevious, "Debug")]
	[InlineData("mauilib", DotNetPrevious, "Release")]
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

		Assert.True(DotnetInternal.New("maui-aspire-servicedefaults", projectDir, additionalDotNetNewParams: $"-n \"{projectName}\" --skipRestore", output: _output),
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

		// Verify the current template was selected and contains the required MAUI properties.
		var project = XDocument.Load(expectedProjectFile);
		var targetFramework = project.Descendants("TargetFramework").Single().Value;
		Assert.Equal(DotNetCurrent, targetFramework);
		Assert.Equal("true", project.Descendants("IsAspireSharedProject").Single().Value);
		Assert.Equal("true", project.Descendants("UseMauiCore").Single().Value);

		var mauiCoreReference = project.Descendants("PackageReference")
			.Single(element => string.Equals((string?)element.Attribute("Include"), "Microsoft.Maui.Core", StringComparison.Ordinal));
		Assert.Equal("$(MauiVersion)", (string?)mauiCoreReference.Attribute("Version"));

		Assert.True(DotnetInternal.Build(expectedProjectFile, "Debug", target: "Restore", properties: BuildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Project {Path.GetFileName(expectedProjectFile)} failed to restore. Check test output/attachments for errors.");

		using var assets = JsonDocument.Parse(File.ReadAllText(Path.Combine(projectDir, "obj", "project.assets.json")));
		var mauiCoreVersion = assets.RootElement
			.GetProperty("project")
			.GetProperty("frameworks")
			.GetProperty(DotNetCurrent)
			.GetProperty("dependencies")
			.GetProperty("Microsoft.Maui.Core")
			.GetProperty("version")
			.GetString();
		Assert.NotNull(mauiCoreVersion);
		Assert.Contains(MauiPackageVersion, mauiCoreVersion, StringComparison.Ordinal);

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
		var commandOutput = DotnetInternal.RunForOutput(
			"new",
			$"maui -o \"{projectDir}\" -f {DotNetCurrent} --with-avalonia --sample-content --no-restore",
			out var exitCode,
			timeoutInSeconds: 300,
			output: _output);
		Assert.Equal(0, exitCode);
		AssertContains("Warning: The Avalonia option was not applied.", commandOutput);
		AssertContains(
			"Avalonia handlers do not currently support the XAML sample content. The generated project includes sample content without Avalonia.",
			commandOutput);
		AssertDoesNotContain("Warning: The sample content option was not applied.", commandOutput);

		Assert.True(File.Exists(projectFile),
			"Unable to create template maui with --with-avalonia --sample-content. Check test output for errors.");

		var csproj = File.ReadAllText(projectFile);
		AssertDoesNotContain("Avalonia.Controls.Maui", csproj);

		var mauiProgram = File.ReadAllText(Path.Combine(projectDir, "MauiProgram.cs"));
		AssertDoesNotContain("UseAvalonia", mauiProgram);

		var appShell = File.ReadAllText(Path.Combine(projectDir, "AppShell.xaml"));
		AssertContains("xmlns:sf=\"clr-namespace:Syncfusion.Maui.Toolkit.SegmentedControl;assembly=Syncfusion.Maui.Toolkit\"", appShell);
		AssertContains("ContentTemplate=\"{DataTemplate MainPage}\"", appShell);
		AssertDoesNotContain("ContentTemplate=\"{DataTemplate local:MainPage}\"", appShell);

		var appShellCodeBehind = File.ReadAllText(Path.Combine(projectDir, "AppShell.xaml.cs"));
		AssertContains("using CommunityToolkit.Maui.Alerts;", appShellCodeBehind);

		var styles = File.ReadAllText(Path.Combine(projectDir, "Resources", "Styles", "Styles.xaml"));
		AssertContains("Syncfusion.Maui.Toolkit.Shimmer", styles);

		var colors = File.ReadAllText(Path.Combine(projectDir, "Resources", "Styles", "Colors.xaml"));
		AssertContains("x:Key=\"DarkBackground\"", colors);
	}

	[Fact]
	public void WithAvaloniaIsIncludedWithCSharpUI()
	{
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		Assert.True(DotnetInternal.New("maui", projectDir, DotNetCurrent, "--ui csharp --with-avalonia --no-restore", output: _output),
			"Unable to create template maui with --ui csharp --with-avalonia. Check test output for errors.");

		var csproj = File.ReadAllText(projectFile);
		AssertContains("Include=\"Avalonia.Controls.Maui\"", csproj);
		AssertContains("Include=\"Avalonia.Controls.Maui.Desktop\"", csproj);

		var mauiProgram = File.ReadAllText(Path.Combine(projectDir, "MauiProgram.cs"));
		AssertContains(".UseAvaloniaApp(useSingleViewLifetime)", mauiProgram);
		AssertContains(".UseAvaloniaEmbedding<AvaloniaApp>()", mauiProgram);

		Assert.True(File.Exists(Path.Combine(projectDir, "MainPage.cs")));
		Assert.False(File.Exists(Path.Combine(projectDir, "MainPage.xaml")));
	}

	[Theory]
	[InlineData("--ui csharp --sample-content --no-restore", false)]
	[InlineData("--ui csharp --sample-content --with-avalonia --no-restore", true)]
	public void SampleContentIgnoredWithCSharpUIIsReported(string options, bool expectAvalonia)
	{
		SetTestIdentifier(options);
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		var commandOutput = DotnetInternal.RunForOutput(
			"new",
			$"maui -o \"{projectDir}\" -f {DotNetCurrent} {options}",
			out var exitCode,
			timeoutInSeconds: 300,
			output: _output);
		Assert.Equal(0, exitCode);
		AssertContains("Warning: The sample content option was not applied.", commandOutput);
		AssertContains(
			"Sample content is only available with XAML. The generated project uses C# UI without sample content.",
			commandOutput);
		AssertDoesNotContain("Warning: The Avalonia option was not applied.", commandOutput);

		Assert.False(Directory.Exists(Path.Combine(projectDir, "Pages")));
		Assert.True(File.Exists(Path.Combine(projectDir, "MainPage.cs")));

		var csproj = File.ReadAllText(projectFile);
		Assert.Equal(expectAvalonia, csproj.Contains("Avalonia.Controls.Maui", StringComparison.Ordinal));
	}
}
