using System.Xml.Linq;

namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class AppleTemplateManifestTests : BaseTemplateTests
{
	const string SceneConfigurationName = "__MAUI_DEFAULT_SCENE_CONFIGURATION__";
	const string SceneDelegateName = "SceneDelegate";

	static readonly string[] SupportedOrientations =
	[
		"UIInterfaceOrientationPortrait",
		"UIInterfaceOrientationLandscapeLeft",
		"UIInterfaceOrientationLandscapeRight",
	];

	static readonly string[] SupportedIPadOrientations =
	[
		"UIInterfaceOrientationPortrait",
		"UIInterfaceOrientationPortraitUpsideDown",
		"UIInterfaceOrientationLandscapeLeft",
		"UIInterfaceOrientationLandscapeRight",
	];

	public static TheoryData<string, string, string, bool, bool> TemplateCases => new()
	{
		{ "MauiDefault", "maui", "--no-restore", true, true },
		{ "MauiSampleContent", "maui", "--sample-content --no-restore", true, true },
		{ "MauiBlazorDefault", "maui-blazor", "", true, true },
		{ "MauiBlazorEmpty", "maui-blazor", "--empty", true, true },
		{ "MauiBlazorWebDefault", "maui-blazor-web", "", true, true },
		{ "MauiBlazorWebEmpty", "maui-blazor-web", "--empty --interactivity Auto --use-program-main", true, true },
		{ "MauiMultiProjectDefault", "maui-multiproject", "", true, true },
		{ "MauiMultiProjectIos", "maui-multiproject", "--ios", true, false },
		{ "MauiMultiProjectMacOs", "maui-multiproject", "--macos", false, true },
		{ "MauiMultiProjectAndroid", "maui-multiproject", "--android", false, false },
		{ "MauiMultiProjectWindows", "maui-multiproject", "--windows", false, false },
	};

	public AppleTemplateManifestTests(IntegrationTestFixture fixture, ITestOutputHelper output)
		: base(fixture, output)
	{
	}

	[Theory]
	[MemberData(nameof(TemplateCases))]
	public void GeneratedAppleApplicationTemplateHasSceneLifecycleAndMacCatalystMinimum(
		string testName,
		string template,
		string arguments,
		bool hasIos,
		bool hasMacCatalyst)
	{
		SetTestIdentifier(testName, template, arguments);
		var customHive = Path.Combine(TestDirectory, "template-hive");
		var templatePackage = FindTemplatePackage();
		Assert.True(
			DotnetInternal.InstallTemplate(templatePackage, customHive, _output),
			$"Unable to install template package '{templatePackage}' into isolated hive '{customHive}'.");

		var projectDir = Path.Combine(TestDirectory, "project");
		Assert.True(
			DotnetInternal.New(
				template,
				projectDir,
				DotNetCurrent,
				arguments,
				output: _output,
				customHive: customHive),
			$"Unable to create template case '{testName}'. Check test output for errors.");

		var layout = GetTemplateLayout(template);
		AssertGeneratedProject(layout, projectDir);
		if (hasMacCatalyst)
			AssertMacCatalystMinimum(layout, projectDir);
		AssertApplePlatform(testName, layout, projectDir, ApplePlatform.iOS, hasIos);
		AssertApplePlatform(testName, layout, projectDir, ApplePlatform.MacCatalyst, hasMacCatalyst);
	}

	string FindTemplatePackage()
	{
		var majorVersion = DotNetCurrent["net".Length..].Split('.')[0];
		var packagePrefix = $"Microsoft.Maui.Templates.net{majorVersion}.";
		var packageOverride = Environment.GetEnvironmentVariable("MAUI_TEMPLATE_TEST_PACKAGE");

		if (!string.IsNullOrWhiteSpace(packageOverride))
		{
			var overridePackagePath = Path.GetFullPath(packageOverride);
			var packageFileName = Path.GetFileName(overridePackagePath);
			Assert.True(File.Exists(overridePackagePath), $"Template package override '{overridePackagePath}' does not exist.");
			Assert.True(
				packageFileName.StartsWith(packagePrefix, StringComparison.OrdinalIgnoreCase) &&
					packageFileName.EndsWith(".nupkg", StringComparison.OrdinalIgnoreCase),
				$"Template package override '{overridePackagePath}' is not a {packagePrefix}*.nupkg package.");

			_output.WriteLine($"Using template package override: {overridePackagePath}");
			return overridePackagePath;
		}

		var expectedFileName = $"Microsoft.Maui.Templates.net{majorVersion}.{MauiPackageVersion}.nupkg";
		var mauiDirectory = TestEnvironment.GetMauiDirectory();
		var packagePath = Path.Combine(mauiDirectory, "artifacts", expectedFileName);
		Assert.True(
			File.Exists(packagePath),
			$"Unable to find exact current template artifact '{packagePath}'. Set MAUI_TEMPLATE_TEST_PACKAGE only for an explicit local package.");

		_output.WriteLine($"Using current template artifact: {packagePath}");
		return packagePath;
	}

	static TemplateLayout GetTemplateLayout(string template) =>
		template switch
		{
			"maui" or "maui-blazor" => TemplateLayout.SingleProject,
			"maui-blazor-web" => TemplateLayout.BlazorWeb,
			"maui-multiproject" => TemplateLayout.MultiProject,
			_ => throw new ArgumentOutOfRangeException(nameof(template)),
		};

	static void AssertGeneratedProject(TemplateLayout layout, string projectDir)
	{
		var projectName = Path.GetFileName(projectDir);
		var projectFile = layout switch
		{
			TemplateLayout.SingleProject => Path.Combine(projectDir, $"{projectName}.csproj"),
			TemplateLayout.BlazorWeb or TemplateLayout.MultiProject => Path.Combine(projectDir, projectName, $"{projectName}.csproj"),
			_ => throw new ArgumentOutOfRangeException(nameof(layout)),
		};

		Assert.True(File.Exists(projectFile), $"Generated template is missing primary project '{projectFile}'.");

		if (layout == TemplateLayout.MultiProject)
		{
			var solutionFile = Path.Combine(projectDir, $"{projectName}.sln");
			Assert.True(File.Exists(solutionFile), $"Generated template is missing solution '{solutionFile}'.");
		}
	}

	static void AssertMacCatalystMinimum(TemplateLayout layout, string projectDir)
	{
		var projectName = Path.GetFileName(projectDir);
		var projectFile = layout switch
		{
			TemplateLayout.SingleProject => Path.Combine(projectDir, $"{projectName}.csproj"),
			TemplateLayout.BlazorWeb => Path.Combine(projectDir, projectName, $"{projectName}.csproj"),
			TemplateLayout.MultiProject => Path.Combine(projectDir, $"{projectName}.Mac", $"{projectName}.Mac.csproj"),
			_ => throw new ArgumentOutOfRangeException(nameof(layout)),
		};

		Assert.True(File.Exists(projectFile), $"Generated template is missing MacCatalyst app project '{projectFile}'.");

		var project = XDocument.Load(projectFile);
		var minimums = project
			.Descendants("SupportedOSPlatformVersion")
			.Where(element =>
				layout == TemplateLayout.MultiProject ||
				(element.Attribute("Condition")?.Value.Contains("'maccatalyst'", StringComparison.OrdinalIgnoreCase) ?? false))
			.ToArray();

		var minimum = Assert.Single(minimums);
		Assert.Equal("17.0", minimum.Value);
	}

	static void AssertApplePlatform(
		string testName,
		TemplateLayout layout,
		string projectDir,
		ApplePlatform platform,
		bool expected)
	{
		var platformDirectory = GetPlatformDirectory(layout, projectDir, platform);
		var plistPath = Path.Combine(platformDirectory, "Info.plist");
		var sceneDelegatePath = Path.Combine(platformDirectory, "SceneDelegate.cs");

		if (!expected)
		{
			Assert.False(File.Exists(plistPath), $"Template case '{testName}' should not contain '{plistPath}'.");
			Assert.False(File.Exists(sceneDelegatePath), $"Template case '{testName}' should not contain '{sceneDelegatePath}'.");
			return;
		}

		Assert.True(File.Exists(plistPath), $"Template case '{testName}' is missing '{plistPath}'.");
		Assert.True(File.Exists(sceneDelegatePath), $"Template case '{testName}' is missing '{sceneDelegatePath}'.");

		var sceneDelegate = File.ReadAllText(sceneDelegatePath);
		Assert.True(
			sceneDelegate.Contains("[Register(\"SceneDelegate\")]", StringComparison.Ordinal),
			$"Scene delegate '{sceneDelegatePath}' must register as '{SceneDelegateName}'.");
		Assert.True(
			sceneDelegate.Contains("public class SceneDelegate : MauiUISceneDelegate", StringComparison.Ordinal),
			$"Scene delegate '{sceneDelegatePath}' must be public and derive from MauiUISceneDelegate.");

		AssertSceneManifest(plistPath, platform, layout);
	}

	static string GetPlatformDirectory(TemplateLayout layout, string projectDir, ApplePlatform platform)
	{
		var projectName = Path.GetFileName(projectDir);

		return layout switch
		{
			TemplateLayout.SingleProject => Path.Combine(projectDir, "Platforms", GetPlatformFolderName(platform)),
			TemplateLayout.BlazorWeb => Path.Combine(projectDir, projectName, "Platforms", GetPlatformFolderName(platform)),
			TemplateLayout.MultiProject => Path.Combine(projectDir, $"{projectName}.{(platform == ApplePlatform.iOS ? "iOS" : "Mac")}"),
			_ => throw new ArgumentOutOfRangeException(nameof(layout)),
		};
	}

	static string GetPlatformFolderName(ApplePlatform platform) =>
		platform == ApplePlatform.iOS ? "iOS" : "MacCatalyst";

	static void AssertSceneManifest(string plistPath, ApplePlatform platform, TemplateLayout layout)
	{
		var document = XDocument.Load(plistPath);
		Assert.NotNull(document.Root);
		Assert.Equal("plist", document.Root!.Name.LocalName);
		Assert.Equal("1.0", document.Root.Attribute("version")?.Value);

		var rootElement = Assert.Single(document.Root.Elements());
		Assert.Equal("dict", rootElement.Name.LocalName);
		var root = ReadDictionary(rootElement, plistPath);

		AssertPreservedValues(root, plistPath, platform, layout);

		var sceneManifest = ReadDictionary(GetValue(root, "UIApplicationSceneManifest", plistPath), "UIApplicationSceneManifest");
		Assert.Equal(2, sceneManifest.Count);
		Assert.Equal("false", GetValue(sceneManifest, "UIApplicationSupportsMultipleScenes", plistPath).Name.LocalName);

		var configurations = ReadDictionary(GetValue(sceneManifest, "UISceneConfigurations", plistPath), "UISceneConfigurations");
		Assert.Single(configurations);
		var applicationRole = GetValue(configurations, "UIWindowSceneSessionRoleApplication", plistPath);
		Assert.Equal("array", applicationRole.Name.LocalName);

		var configuration = Assert.Single(applicationRole.Elements());
		var configurationValues = ReadDictionary(configuration, "UIWindowSceneSessionRoleApplication");
		Assert.Equal(2, configurationValues.Count);
		Assert.Equal(SceneConfigurationName, GetString(configurationValues, "UISceneConfigurationName", plistPath));
		Assert.Equal(SceneDelegateName, GetString(configurationValues, "UISceneDelegateClassName", plistPath));
		Assert.False(configurationValues.ContainsKey("UISceneStoryboardFile"), $"Property-list '{plistPath}' must not declare UISceneStoryboardFile.");
	}

	static void AssertPreservedValues(
		IReadOnlyDictionary<string, XElement> root,
		string plistPath,
		ApplePlatform platform,
		TemplateLayout layout)
	{
		var expectedDeviceFamily = platform == ApplePlatform.iOS ? new[] { "1", "2" } : new[] { "2" };
		Assert.Equal(expectedDeviceFamily, GetArrayValues(root, "UIDeviceFamily", "integer", plistPath));
		Assert.Equal(SupportedOrientations, GetArrayValues(root, "UISupportedInterfaceOrientations", "string", plistPath));

		if (platform == ApplePlatform.iOS)
		{
			Assert.Equal("true", GetValue(root, "LSRequiresIPhoneOS", plistPath).Name.LocalName);
			Assert.Equal(SupportedIPadOrientations, GetArrayValues(root, "UISupportedInterfaceOrientations~ipad", "string", plistPath));
		}

		var appIcon = GetString(root, "XSAppIconAssets", plistPath);
		var expectedAppIcon = layout == TemplateLayout.MultiProject
			? "Assets.xcassets/AppIcon.appiconset"
			: "Assets.xcassets/appicon.appiconset";
		Assert.Equal(expectedAppIcon, appIcon);

		if (layout == TemplateLayout.MultiProject)
		{
			Assert.False(string.IsNullOrWhiteSpace(GetString(root, "CFBundleDisplayName", plistPath)));
			Assert.StartsWith("com.companyname.", GetString(root, "CFBundleIdentifier", plistPath), StringComparison.Ordinal);
			Assert.Equal("1.0", GetString(root, "CFBundleShortVersionString", plistPath));
			Assert.Equal("1.0", GetString(root, "CFBundleVersion", plistPath));
		}
	}

	static IReadOnlyDictionary<string, XElement> ReadDictionary(XElement dictionary, string context)
	{
		Assert.Equal("dict", dictionary.Name.LocalName);
		var elements = dictionary.Elements().ToArray();
		Assert.True(elements.Length % 2 == 0, $"Property-list dictionary '{context}' must contain key/value pairs.");

		var values = new Dictionary<string, XElement>(StringComparer.Ordinal);
		for (var index = 0; index < elements.Length; index += 2)
		{
			var key = elements[index];
			Assert.Equal("key", key.Name.LocalName);
			Assert.True(values.TryAdd(key.Value, elements[index + 1]), $"Duplicate property-list key '{key.Value}' in '{context}'.");
		}

		return values;
	}

	static XElement GetValue(IReadOnlyDictionary<string, XElement> dictionary, string key, string context)
	{
		Assert.True(dictionary.TryGetValue(key, out var value), $"Property-list '{context}' is missing key '{key}'.");
		return value!;
	}

	static string GetString(IReadOnlyDictionary<string, XElement> dictionary, string key, string context)
	{
		var value = GetValue(dictionary, key, context);
		Assert.Equal("string", value.Name.LocalName);
		return value.Value;
	}

	static string[] GetArrayValues(
		IReadOnlyDictionary<string, XElement> dictionary,
		string key,
		string valueElementName,
		string context)
	{
		var value = GetValue(dictionary, key, context);
		Assert.Equal("array", value.Name.LocalName);
		var values = value.Elements().ToArray();
		Assert.All(values, item => Assert.Equal(valueElementName, item.Name.LocalName));
		return values.Select(item => item.Value).ToArray();
	}

	enum TemplateLayout
	{
		SingleProject,
		BlazorWeb,
		MultiProject,
	}

	enum ApplePlatform
	{
		iOS,
		MacCatalyst,
	}
}
