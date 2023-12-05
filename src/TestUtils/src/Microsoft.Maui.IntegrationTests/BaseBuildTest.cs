
namespace Microsoft.Maui.IntegrationTests
{
	public class BaseBuildTest
	{
		public const string DotNetCurrent = "net8.0";
		public const string DotNetPrevious = "net7.0";

		public const string MauiVersionCurrent = "8.0.0-rc.1.9171"; // this should not be the same as the last release
		public const string MauiVersionPrevious = "7.0.86"; // this should not be the same version as the default. aka: MicrosoftMauiPreviousDotNetReleasedVersion in eng/Versions.props

		char[] invalidChars = { '{', '}', '(', ')', '$', ':', ';', '\"', '\'', ',', '=', '.', '-', };

		public string TestName
		{
			get
			{
				var result = TestContext.CurrentContext.Test.Name;
				foreach (var c in invalidChars.Concat(Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars())))
				{
					result = result.Replace(c, '_');
				}
				return result.Replace("_", string.Empty, StringComparison.OrdinalIgnoreCase);
			}
		}

		public string LogDirectory => Path.Combine(TestEnvironment.GetLogDirectory(), TestName);

		public string TestDirectory => Path.Combine(TestEnvironment.GetTestDirectoryRoot(), TestName);

		public string TestNuGetConfig => Path.Combine(TestEnvironment.GetTestDirectoryRoot(), "NuGet.config");

		// Properties that ensure we don't use cached packages, and *only* the empty NuGet.config
		protected List<string> BuildProps => new()
		{
			"RestoreNoCache=true",
			//"GenerateAppxPackageOnBuild=true",
			$"RestorePackagesPath={Path.Combine(TestEnvironment.GetTestDirectoryRoot(), "packages")}",
			$"RestoreConfigFile={TestNuGetConfig}",
			// Avoid iOS build warning as error on Windows: There is no available connection to the Mac. Task 'VerifyXcodeVersion' will not be executed
			$"CustomBeforeMicrosoftCSharpTargets={Path.Combine(TestEnvironment.GetMauiDirectory(), "src", "Templates", "TemplateTestExtraTargets.targets")}",
			//Try not restore dependencies of 6.0.10
			$"DisableTransitiveFrameworkReferenceDownloads=true",
			// Surface warnings as build errors
			"TreatWarningsAsErrors=true",
		};


		/// <summary>
		/// Copy NuGet packages that are not installed as part of the workload and set up NuGet.config
		/// See: `PrepareSeparateBuildContext` in `eng/cake/dotnet.cake`.
		/// TODO: Should these be moved to a library-packs workload folder for testing?
		/// </summary>
		/// <exception cref="DirectoryNotFoundException"></exception>
		[OneTimeSetUp]
		public void BuildTestFxtSetUp()
		{
			string[] NuGetOnlyPackages = new string[] {
				"Microsoft.Maui.Controls.*.nupkg",
				"Microsoft.Maui.Core.*.nupkg",
				"Microsoft.Maui.Essentials.*.nupkg",
				"Microsoft.Maui.Graphics.*.nupkg",
				"Microsoft.Maui.Maps.*.nupkg",
				"Microsoft.AspNetCore.Components.WebView.*.nupkg",
			};

			var artifactDir = Path.Combine(TestEnvironment.GetMauiDirectory(), "artifacts");
			if (!Directory.Exists(artifactDir))
				throw new DirectoryNotFoundException($"Build artifact directory '{artifactDir}' was not found.");

			var extraPacksDir = Path.Combine(TestEnvironment.GetTestDirectoryRoot(), "extra-packages");
			if (Directory.Exists(extraPacksDir))
				Directory.Delete(extraPacksDir, true);

			Directory.CreateDirectory(extraPacksDir);

			foreach (var searchPattern in NuGetOnlyPackages)
			{
				foreach (var pack in Directory.GetFiles(artifactDir, searchPattern))
					File.Copy(pack, Path.Combine(extraPacksDir, Path.GetFileName(pack)));
			}

			File.Copy(Path.Combine(TestEnvironment.GetMauiDirectory(), "NuGet.config"), TestNuGetConfig, true);
			FileUtilities.ReplaceInFile(TestNuGetConfig,
				"<!-- <add key=\"local\" value=\"artifacts\" /> -->",
				$"<add key=\"nuget-only\" value=\"{extraPacksDir}\" />");
		}

		[SetUp]
		public void BuildTestSetUp()
		{
			if (Directory.Exists(TestDirectory))
				Directory.Delete(TestDirectory, recursive: true);

			Directory.CreateDirectory(TestDirectory);
		}

		[OneTimeTearDown]
		public void BuildTestFxtTearDown() { }

		[TearDown]
		public void BuildTestTearDown()
		{
			// Attach test content and logs as artifacts
			foreach (var log in Directory.GetFiles(Path.Combine(TestDirectory), "*log", SearchOption.AllDirectories))
			{
				TestContext.AddTestAttachment(log, Path.GetFileName(TestDirectory));
			}
		}

	}
}
