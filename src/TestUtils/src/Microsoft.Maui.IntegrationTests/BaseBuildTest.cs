using System.Globalization;

namespace Microsoft.Maui.IntegrationTests
{
	public enum RuntimeVariant
	{
		Mono,
		NativeAOT
	}

	public abstract class BaseBuildTest
	{
		public const string DotNetCurrent = "net10.0";
		public const string DotNetPrevious = "net9.0";

		// Versions of .NET MAUI that are used when testing the <MauiVersion> property. These should preferrably
		// different to the defaults in the SDKs such that the tests can test what would happen if the user puts
		// some arbitrary number in <MauiVersion>. The actual numbers do not matter as much, as long as they trigger
		// the MSBuild targets that would download some version that is only on nuget.org and not in the workload.
		//
		// MauiVersionCurrent: this should be the current .NET version of MAUI, but the latest released build.
		// For example, if this branch is for .NET 9, then this must be a 9.0.x number. If the latest MAUI release
		// is 9.0.100, then this should preferrable be some older build to make sure things work, like 9.0.30.
		public const string MauiVersionCurrent = "";
		// MauiVersionPrevious: this should be the previous .NET version of MAUI.
		// For example, if this branch is for .NET 9, then this must be a 8.0.x number, but should preferrably
		// not be the same as the default in MicrosoftMauiPreviousDotNetReleasedVersion in eng/Versions.props
		// as this would result in the tests not testing anything. If the .NET 9 version of MAUI pulls in 8.0.100
		// of the .NET 8 MAUI, then this should be 8.0.80 for example.
		public const string MauiVersionPrevious = "9.0.82";

		char[] invalidChars = { '{', '}', '(', ')', '$', ':', ';', '\"', '\'', ',', '=', '.', '-', ' ', };

		public string MauiPackageVersion
		{
			get
			{
				var version = Environment.GetEnvironmentVariable("MAUI_PACKAGE_VERSION");
				if (string.IsNullOrWhiteSpace(version))
					throw new Exception("MAUI_PACKAGE_VERSION was not set.");
				return version;
			}
		}

		public string TestName
		{
			get
			{
				var result = TestContext.CurrentContext.Test.Name;
				foreach (var c in invalidChars.Concat(Path.GetInvalidPathChars().Concat(Path.GetInvalidFileNameChars())))
				{
					result = result.Replace(c, '_');
				}
				result = result.Replace("_", string.Empty, StringComparison.OrdinalIgnoreCase);

				if (result.Length > 20)
				{
					// If the test name is too long, hash it to avoid path length issues
					result = result.Substring(0, 15) + Convert.ToString(Math.Abs(string.GetHashCode(result.AsSpan(), StringComparison.Ordinal)), CultureInfo.InvariantCulture);
				}
				return result;
			}
		}

		public string LogDirectory => Path.Combine(TestEnvironment.GetLogDirectory(), TestName);

		public string TestDirectory => Path.Combine(TestEnvironment.GetTestDirectoryRoot(), TestName);

		public string TestNuGetConfig => Path.Combine(TestEnvironment.GetTestDirectoryRoot(), "NuGet.config");

		// Properties that ensure we don't use cached packages, and *only* the empty NuGet.config
		protected List<string> BuildProps
		{
			get
			{
				var props = new List<string>
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
					// Detailed trimmer warnings, if present
					"TrimmerSingleWarn=false",
				};

				// Allow disabling Xcode version validation for cases where Xcode version is newer than SDK expects
				var skipXcodeValidation = Environment.GetEnvironmentVariable("SKIP_XCODE_VERSION_CHECK");
				if (!string.IsNullOrEmpty(skipXcodeValidation) && skipXcodeValidation.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					props.Add("ValidateXcodeVersion=false");
				}

				// Prevent output path redirection from repo's Directory.Build.props
				props.Add("UseCommonOutputDirectory=false");

				return props;
			}
		}


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
				"Microsoft.Maui.Resizetizer.*.nupkg",
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
			FileUtilities.ReplaceInFile(TestNuGetConfig, "<add key=\"nuget-only\" value=\"true\" />", "");
			FileUtilities.ReplaceInFile(TestNuGetConfig, "NUGET_ONLY_PLACEHOLDER", extraPacksDir);

			// Create empty Directory.Build.props and Directory.Build.targets to prevent inheritance
			// from the repo's root files which would redirect output paths to artifacts/
			var testDirRoot = TestEnvironment.GetTestDirectoryRoot();
			File.WriteAllText(Path.Combine(testDirRoot, "Directory.Build.props"), "<Project />");
			File.WriteAllText(Path.Combine(testDirRoot, "Directory.Build.targets"), "<Project />");
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
