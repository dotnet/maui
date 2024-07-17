using System.Xml.Linq;
using Microsoft.Maui.IntegrationTests.Apple;

namespace Microsoft.Maui.IntegrationTests
{
	public class TemplateTests : BaseBuildTest
	{
		[SetUp]
		public void TemplateTestsSetUp()
		{
			File.Copy(Path.Combine(TestEnvironment.GetMauiDirectory(), "src", "Templates", "tests", "Directory.Build.props"),
				Path.Combine(TestDirectory, "Directory.Build.props"), true);
			File.Copy(Path.Combine(TestEnvironment.GetMauiDirectory(), "src", "Templates", "tests", "Directory.Build.targets"),
				Path.Combine(TestDirectory, "Directory.Build.targets"), true);
		}

		[Test]
		// Parameters: short name, target framework, build config, use pack target, additionalDotNetNewParams
		//[TestCase("maui", DotNetPrevious, "Debug", false, "")]
		//[TestCase("maui", DotNetPrevious, "Release", false, "")]
		[TestCase("maui", DotNetCurrent, "Debug", false, "")]
		[TestCase("maui", DotNetCurrent, "Release", false, "")]
		//[TestCase("maui-blazor", DotNetPrevious, "Debug", false, "")]
		//[TestCase("maui-blazor", DotNetPrevious, "Release", false, "")]
		[TestCase("maui-blazor", DotNetCurrent, "Debug", false, "")]
		[TestCase("maui-blazor", DotNetCurrent, "Release", false, "")]
		[TestCase("maui-blazor", DotNetCurrent, "Debug", false, "--Empty")]
		[TestCase("maui-blazor", DotNetCurrent, "Release", false, "--Empty")]
		[TestCase("mauilib", DotNetPrevious, "Debug", true, "")]
		[TestCase("mauilib", DotNetPrevious, "Release", true, "")]
		[TestCase("mauilib", DotNetCurrent, "Debug", true, "")]
		[TestCase("mauilib", DotNetCurrent, "Release", true, "")]
		public void Build(string id, string framework, string config, bool shouldPack, string additionalDotNetNewParams)
		{
			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir, framework, additionalDotNetNewParams),
				$"Unable to create template {id}. Check test output for errors.");

			EnableTizen(projectFile);

			if (shouldPack)
				FileUtilities.ReplaceInFile(projectFile,
					"</Project>",
					"<PropertyGroup><Version>1.0.0-preview.1</Version></PropertyGroup></Project>");

			string target = shouldPack ? "Pack" : "";
			Assert.IsTrue(DotnetInternal.Build(projectFile, config, target: target, properties: BuildProps, msbuildWarningsAsErrors: true),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
		}

		[Test]
		// Parameters:  target framework, build config, dotnet new additional parameters

		// First, default scenarios
		[TestCase(DotNetCurrent, "Debug", "", false)]
		[TestCase(DotNetCurrent, "Release", "", false)]

		// Then, scenarios with additional template parameters:
		// - Interactivity Location: None/WASM/Server/Auto
		// - Empty vs. With Sample Content
		// - ProgramMain vs. TopLevel statements
		// And alternately testing other options for a healthy mix.
		[TestCase(DotNetCurrent, "Debug", "-I None --Empty", false)]
		[TestCase(DotNetCurrent, "Release", "-I WebAssembly --Empty", false)]
		[TestCase(DotNetCurrent, "Debug", "-I Server --Empty", false)]
		[TestCase(DotNetCurrent, "Release", "-I Auto --Empty", false)]
		[TestCase(DotNetCurrent, "Debug", "-I None", false)]
		[TestCase(DotNetCurrent, "Release", "-I WebAssembly", false)]
		[TestCase(DotNetCurrent, "Debug", "-I Server", false)]
		[TestCase(DotNetCurrent, "Release", "-I Auto", false)]
		[TestCase(DotNetCurrent, "Debug", "-I None --Empty --UseProgramMain", false)]
		[TestCase(DotNetCurrent, "Release", "-I WebAssembly --Empty --UseProgramMain", false)]
		[TestCase(DotNetCurrent, "Debug", "-I Server --Empty --UseProgramMain", false)]
		[TestCase(DotNetCurrent, "Release", "-I Auto --Empty --UseProgramMain", false)]
		[TestCase(DotNetCurrent, "Debug", "-I None --UseProgramMain", false)]
		[TestCase(DotNetCurrent, "Release", "-I WebAssembly --UseProgramMain", false)]
		[TestCase(DotNetCurrent, "Debug", "-I Server --UseProgramMain", false)]
		[TestCase(DotNetCurrent, "Release", "-I Auto --UseProgramMain", false)]

		// Then, some scenarios with tricky names in Debug builds only
		// This doesn't work on Android in Release, so we skip that for now
		// See https://github.com/dotnet/android/issues/9107
		[TestCase(DotNetCurrent, "Debug", "", true)]
		[TestCase(DotNetCurrent, "Debug", "-I Server --UseProgramMain", true)]
		public void BuildMauiBlazorWebSolution(string framework, string config, string additionalDotNetNewParams, bool useTrickyProjectName)
		{
			const string templateShortName = "maui-blazor-web";

			var solutionProjectDir = TestDirectory;
   			if (useTrickyProjectName)
	  		{
	 			solutionProjectDir += "&More";
	 		}

			var webAppProjectDir = Path.Combine(solutionProjectDir, Path.GetFileName(solutionProjectDir) + ".Web");
			var webAppProjectFile = Path.Combine(webAppProjectDir, $"{Path.GetFileName(webAppProjectDir)}.csproj");

			var mauiAppProjectDir = Path.Combine(solutionProjectDir, Path.GetFileName(solutionProjectDir));
			var mauiAppProjectFile = Path.Combine(mauiAppProjectDir, $"{Path.GetFileName(mauiAppProjectDir)}.csproj");

			TestContext.WriteLine($"Creating project in {solutionProjectDir}");

			Assert.IsTrue(DotnetInternal.New(templateShortName, outputDirectory: solutionProjectDir, framework: framework, additionalDotNetNewParams: additionalDotNetNewParams),
				$"Unable to create template {templateShortName}. Check test output for errors.");

			TestContext.WriteLine($"Solution directory: {solutionProjectDir} (exists? {Directory.Exists(solutionProjectDir)})");
			TestContext.WriteLine($"Blazor Web app project directory: {webAppProjectDir} (exists? {Directory.Exists(webAppProjectDir)})");
			TestContext.WriteLine($"Blazor Web app project file: {webAppProjectFile} (exists? {File.Exists(webAppProjectFile)})");
			TestContext.WriteLine($"MAUI app project directory: {mauiAppProjectDir} (exists? {Directory.Exists(mauiAppProjectDir)})");
			TestContext.WriteLine($"MAUI app project file: {mauiAppProjectFile} (exists? {File.Exists(mauiAppProjectFile)})");

			EnableTizen(mauiAppProjectFile);

			TestContext.WriteLine($"Building Blazor Web app: {webAppProjectFile}");
			Assert.IsTrue(DotnetInternal.Build(webAppProjectFile, config, target: "", properties: BuildProps, msbuildWarningsAsErrors: true),
				$"Project {Path.GetFileName(webAppProjectFile)} failed to build. Check test output/attachments for errors.");

			TestContext.WriteLine($"Building .NET MAUI app: {mauiAppProjectFile}");
			Assert.IsTrue(DotnetInternal.Build(mauiAppProjectFile, config, target: "", properties: BuildProps, msbuildWarningsAsErrors: true),
				$"Project {Path.GetFileName(mauiAppProjectFile)} failed to build. Check test output/attachments for errors.");
		}

		[Test]
		[TestCase("Debug", "simplemulti")]
		[TestCase("Release", "simplemulti")]
		[TestCase("Debug", "MultiProject@Symbol & More")]
		[TestCase("Release", "MultiProject@Symbol & More")]
		public void BuildMultiProject(string config, string projectName)
		{
			var projectDir = Path.Combine(TestDirectory, projectName);
			var name = Path.GetFileName(projectDir);
			var solutionFile = Path.Combine(projectDir, $"{name}.sln");

			Assert.IsTrue(DotnetInternal.New("maui-multiproject", projectDir, DotNetCurrent),
				$"Unable to create template maui-multiproject. Check test output for errors.");

			if (!TestEnvironment.IsWindows)
			{
				Assert.IsTrue(DotnetInternal.Run("sln", $"\"{solutionFile}\" remove \"{projectDir}/{name}.WinUI/{name}.WinUI.csproj\""),
					$"Unable to remove WinUI project from solution. Check test output for errors.");
			}

			Assert.IsTrue(DotnetInternal.Build(solutionFile, config, properties: BuildProps, msbuildWarningsAsErrors: true),
				$"Solution {name} failed to build. Check test output/attachments for errors.");
		}

		[Test]
		[TestCase("Debug", "--android")]
		[TestCase("Debug", "--ios")]
		[TestCase("Debug", "--windows")]
		[TestCase("Debug", "--macos")]
		public void BuildMultiProjectSinglePlatform(string config, string platformArg)
		{
			var projectDir = TestDirectory;
			var name = Path.GetFileName(projectDir);
			var solutionFile = Path.Combine(projectDir, $"{name}.sln");

			Assert.IsTrue(DotnetInternal.New($"maui-multiproject {platformArg}", projectDir, DotNetCurrent),
				$"Unable to create template maui-multiproject. Check test output for errors.");

			if (!TestEnvironment.IsWindows)
			{
				Assert.IsTrue(DotnetInternal.Run("sln", $"{solutionFile} remove {projectDir}/{name}.WinUI/{name}.WinUI.csproj"),
					$"Unable to remove WinUI project from solution. Check test output for errors.");
			}

			Assert.IsTrue(DotnetInternal.Build(solutionFile, config, properties: BuildProps, msbuildWarningsAsErrors: true),
				$"Solution {name} failed to build. Check test output/attachments for errors.");
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
		// Parameters: short name, target framework, build config, use pack target
		//[TestCase("maui", DotNetPrevious, "Debug", false)]
		//[TestCase("maui", DotNetPrevious, "Release", false)]
		[TestCase("maui", DotNetCurrent, "Debug", false)]
		[TestCase("maui", DotNetCurrent, "Release", false)]
		//[TestCase("maui-blazor", DotNetPrevious, "Debug", false)]
		//[TestCase("maui-blazor", DotNetPrevious, "Release", false)]
		[TestCase("maui-blazor", DotNetCurrent, "Debug", false)]
		[TestCase("maui-blazor", DotNetCurrent, "Release", false)]
		[TestCase("mauilib", DotNetPrevious, "Debug", true)]
		[TestCase("mauilib", DotNetPrevious, "Release", true)]
		[TestCase("mauilib", DotNetCurrent, "Debug", true)]
		[TestCase("mauilib", DotNetCurrent, "Release", true)]
		public void BuildWithMauiVersion(string id, string framework, string config, bool shouldPack)
		{
			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
				$"Unable to create template {id}. Check test output for errors.");

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

			string target = shouldPack ? "Pack" : "";
			Assert.IsTrue(DotnetInternal.Build(projectFile, config, target: target, properties: BuildProps),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
		}

		[Test]
		//[TestCase("maui", DotNetPrevious, "Debug")]
		//[TestCase("maui", DotNetPrevious, "Release")]
		[TestCase("maui", DotNetCurrent, "Debug")]
		[TestCase("maui", DotNetCurrent, "Release")]
		//[TestCase("maui-blazor", DotNetPrevious, "Debug")]
		//[TestCase("maui-blazor", DotNetPrevious, "Release")]
		[TestCase("maui-blazor", DotNetCurrent, "Debug")]
		[TestCase("maui-blazor", DotNetCurrent, "Release")]
		public void BuildUnpackaged(string id, string framework, string config)
		{
			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
				$"Unable to create template {id}. Check test output for errors.");

			EnableTizen(projectFile);
			FileUtilities.ReplaceInFile(projectFile,
				"<UseMaui>true</UseMaui>",
				"<UseMaui>true</UseMaui><WindowsPackageType>None</WindowsPackageType>");

			Assert.IsTrue(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
		}

		[Test]
		[TestCase("maui", true, true)]
		[TestCase("maui", true, false)]
		[TestCase("maui", false, true)]
		public void BuildWindowsAppSDKSelfContained(string id, bool wasdkself, bool netself)
		{
			if (TestEnvironment.IsMacOS)
				Assert.Ignore("This test is designed for testing a windows build.");

			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir, DotNetCurrent),
				$"Unable to create template {id}. Check test output for errors.");

			FileUtilities.ReplaceInFile(projectFile,
				"<UseMaui>true</UseMaui>",
				$"""
				<UseMaui>true</UseMaui>
				<WindowsAppSDKSelfContained>{wasdkself}</WindowsAppSDKSelfContained>
				<SelfContained>{netself}</SelfContained>
				""");

			var extendedBuildProps = BuildProps;
			extendedBuildProps.Add($"TargetFramework={DotNetCurrent}-windows10.0.19041.0");

			Assert.IsTrue(DotnetInternal.Build(projectFile, "Release", properties: extendedBuildProps, msbuildWarningsAsErrors: true),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
		}

		[Test]
		[TestCase("maui", true, "None")]
		[TestCase("maui", true, "MSIX")]
		[TestCase("maui", false, "None")]
		[TestCase("maui", false, "MSIX")]
		public void BuildWindowsRidGraph(string id, bool useridgraph, string packageType)
		{
			if (TestEnvironment.IsMacOS)
				Assert.Ignore("This test is designed for testing a windows build.");

			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir, DotNetCurrent),
				$"Unable to create template {id}. Check test output for errors.");

			FileUtilities.ReplaceInFile(projectFile,
				"<UseMaui>true</UseMaui>",
				$"""
				<UseMaui>true</UseMaui>
				<UseRidGraph>{useridgraph}</UseRidGraph>
				<WindowsPackageType>{packageType}</WindowsPackageType>
				""");

			var extendedBuildProps = BuildProps;
			extendedBuildProps.Add($"TargetFramework={DotNetCurrent}-windows10.0.19041.0");

			Assert.IsTrue(DotnetInternal.Build(projectFile, "Release", properties: extendedBuildProps, msbuildWarningsAsErrors: true),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
		}

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
			actualWarnings.AssertNoWarnings();
		}

		private List<string> PrepareNativeAotBuildProps()
		{
			var extendedBuildProps = new List<string>(BuildProps);
			extendedBuildProps.Add("PublishAot=true");
			extendedBuildProps.Add("PublishAotUsingRuntimePack=true");  // TODO: This parameter will become obsolete https://github.com/dotnet/runtime/issues/87060 in net9
			extendedBuildProps.Add("_IsPublishing=true"); // This makes 'dotnet build -r iossimulator-x64' equivalent to 'dotnet publish -r iossimulator-x64'
			extendedBuildProps.Add("IlcTreatWarningsAsErrors=false");
			extendedBuildProps.Add("TrimmerSingleWarn=false");
			return extendedBuildProps;
		}

		[Test]
		[TestCase("maui", DotNetCurrent, "Release")]
		public void PublishUnpackaged(string id, string framework, string config)
		{
			if (!TestEnvironment.IsWindows)
				Assert.Ignore("Running Windows templates is only supported on Windows.");

			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
				$"Unable to create template {id}. Check test output for errors.");

			BuildProps.Add("WindowsPackageType=None");

			Assert.IsTrue(DotnetInternal.Publish(projectFile, config, framework: $"{framework}-windows10.0.19041.0", properties: BuildProps),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");

			var assetsRoot = Path.Combine(projectDir, $"bin/{config}/{framework}-windows10.0.19041.0/win10-x64/publish");

			AssetExists("dotnet_bot.scale-100.png");
			AssetExists("appiconLogo.scale-100.png");
			AssetExists("OpenSans-Regular.ttf");
			AssetExists("splashSplashScreen.scale-100.png");
			AssetExists("AboutAssets.txt");

			void AssetExists(string filename)
			{
				var fullpath = Path.Combine(assetsRoot!, filename);
				Assert.IsTrue(File.Exists(fullpath),
					$"Unable to find expected asset: {fullpath}");
			}
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
		[TestCase("maui", "Debug", "2.0", "2")]
		[TestCase("maui", "Release", "2.0", "2")]
		[TestCase("maui", "Release", "0.3", "3")]
		[TestCase("maui-blazor", "Debug", "2.0", "2")]
		[TestCase("maui-blazor", "Release", "2.0", "2")]
		[TestCase("maui-blazor", "Release", "0.3", "3")]
		public void BuildWithDifferentVersionNumber(string id, string config, string display, string version)
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

			Assert.IsTrue(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true),
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

		void EnableTizen(string projectFile)
		{
			FileUtilities.ReplaceInFile(projectFile, new Dictionary<string, string>()
			{
				{ "<!-- <TargetFrameworks>", "<TargetFrameworks>" },
				{ "</TargetFrameworks> -->", "</TargetFrameworks>" },
			});
		}

	}
}
