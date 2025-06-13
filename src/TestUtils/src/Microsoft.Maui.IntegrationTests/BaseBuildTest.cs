using System.Globalization;

namespace Microsoft.Maui.IntegrationTests
{
	public enum RuntimeVariant
	{
		Mono,
		NativeAOT
	}
    
	/// <summary>
	/// Base class for all build tests that replaces the functionality previously provided by NUnit attributes
	/// </summary>
	public abstract class BaseBuildTest : IDisposable
	{
		public const string DotNetCurrent = "net9.0";
		public const string DotNetPrevious = "net8.0";

		public const string MauiVersionCurrent = "9.0.0-rc.1.24453.9"; // this should not be the same as the last release
		public const string MauiVersionPrevious = "8.0.72"; // this should not be the same version as the default. aka: MicrosoftMauiPreviousDotNetReleasedVersion in eng/Versions.props

		private readonly char[] invalidChars = { '{', '}', '(', ')', '$', ':', ';', '\"', '\'', ',', '=', '.', '-', ' ', };
		protected string? _testName;
		protected ITestOutputHelper? _output;

		static BaseBuildTest()
		{
			// This is the one-time setup that would have been in [OneTimeSetUp]
			PrepareNuGetPackages();
		}
        
		/// <summary>
		/// Base constructor for all build tests
		/// </summary>
		public BaseBuildTest(ITestOutputHelper? output = null)
		{
			_output = output;
            
			// Setup for each test
			if (Directory.Exists(TestDirectory))
				Directory.Delete(TestDirectory, recursive: true);

			Directory.CreateDirectory(TestDirectory);
		}

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
				if (_testName == null)
				{
					// Generate a random test name since we don't have TestContext.CurrentContext.Test.Name
					_testName = $"Test_{Guid.NewGuid().ToString("N").Substring(0, 8)}";
				}

				var result = _testName;
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
			set => _testName = value;
		}

		public string LogDirectory => Path.Combine(TestEnvironment.GetLogDirectory(), TestName);

		public string TestDirectory => Path.Combine(TestEnvironment.GetTestDirectoryRoot(), TestName);

		public static string TestNuGetConfig => Path.Combine(TestEnvironment.GetTestDirectoryRoot(), "NuGet.config");

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
		static void PrepareNuGetPackages()
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
		}

		public virtual void Dispose()
		{
			// Equivalent to [TearDown]
			// Attach test content and logs as artifacts
			// Since XUnit doesn't have built-in test attachments, we just log the file locations
			if (_output != null)
			{
				foreach (var log in Directory.GetFiles(Path.Combine(TestDirectory), "*log", SearchOption.AllDirectories))
				{
					_output.WriteLine($"Test log file: {log}");
				}
			}
		}
	}
}
