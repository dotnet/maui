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
		// Parameters: short name, target framework, build config, use pack target
		[TestCase("maui", DotNetPrevious, "Debug", false)]
		[TestCase("maui", DotNetPrevious, "Release", false)]
		[TestCase("maui", DotNetCurrent, "Debug", false)]
		[TestCase("maui", DotNetCurrent, "Release", false)]
		[TestCase("maui-blazor", DotNetPrevious, "Debug", false)]
		[TestCase("maui-blazor", DotNetPrevious, "Release", false)]
		[TestCase("maui-blazor", DotNetCurrent, "Debug", false)]
		[TestCase("maui-blazor", DotNetCurrent, "Release", false)]
		[TestCase("mauilib", DotNetPrevious, "Debug", true)]
		[TestCase("mauilib", DotNetPrevious, "Release", true)]
		[TestCase("mauilib", DotNetCurrent, "Debug", true)]
		[TestCase("mauilib", DotNetCurrent, "Release", true)]
		public void Build(string id, string framework, string config, bool shouldPack)
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

			string target = shouldPack ? "Pack" : "";
			Assert.IsTrue(DotnetInternal.Build(projectFile, config, target: target, properties: BuildProps, msbuildWarningsAsErrors: true),
				$"Project {Path.GetFileName(projectFile)} failed to build. Check test output/attachments for errors.");
		}

		[Test]
		// Parameters: short name, target framework, build config, use pack target
		[TestCase("maui", DotNetPrevious, "Debug", false)]
		[TestCase("maui", DotNetPrevious, "Release", false)]
		[TestCase("maui", DotNetCurrent, "Debug", false)]
		[TestCase("maui", DotNetCurrent, "Release", false)]
		[TestCase("maui-blazor", DotNetPrevious, "Debug", false)]
		[TestCase("maui-blazor", DotNetPrevious, "Release", false)]
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
		[TestCase("maui", DotNetPrevious, "Debug")]
		[TestCase("maui", DotNetPrevious, "Release")]
		[TestCase("maui", DotNetCurrent, "Debug")]
		[TestCase("maui", DotNetCurrent, "Release")]
		[TestCase("maui-blazor", DotNetPrevious, "Debug")]
		[TestCase("maui-blazor", DotNetPrevious, "Release")]
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
			if (framework != "net6.0")
			{
				// On versions after net6.0 this package reference also has to be updated to ensure the version of the MAUI Core package
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

			List<string> expectedEntitlements = config == "Release" ?
				new() { "com.apple.security.app-sandbox", "com.apple.security.network.client" } :
				new() { "com.apple.security.get-task-allow" };
			List<string> foundEntitlements = Codesign.SearchForExpectedEntitlements(entitlementsPath, appLocation, expectedEntitlements);

			CollectionAssert.AreEqual(expectedEntitlements, foundEntitlements, "Entitlements missing from executable.");
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
