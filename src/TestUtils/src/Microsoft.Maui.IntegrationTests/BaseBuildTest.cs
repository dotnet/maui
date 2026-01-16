using System.Globalization;
using System.Reflection;

namespace Microsoft.Maui.IntegrationTests
{
	public enum RuntimeVariant
	{
		Mono,
		NativeAOT
	}

	// XUnit class fixture for one-time setup and teardown
	public class IntegrationTestFixture : IDisposable
	{
		public string TestNuGetConfig { get; }

		public IntegrationTestFixture()
		{
			TestNuGetConfig = Path.Combine(TestEnvironment.GetTestDirectoryRoot(), "NuGet.config");
			SetUpNuGetPackages();
		}

		/// <summary>
		/// Copy NuGet packages that are not installed as part of the workload and set up NuGet.config
		/// See: `PrepareSeparateBuildContext` in `eng/cake/dotnet.cake`.
		/// </summary>
		private void SetUpNuGetPackages()
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

			var mauiDir = TestEnvironment.GetMauiDirectory();
			var artifactDir = Path.Combine(mauiDir, "artifacts");
			if (!Directory.Exists(artifactDir))
				throw new DirectoryNotFoundException($"Build artifact directory '{artifactDir}' was not found.");

			var extraPacksDir = Path.Combine(TestEnvironment.GetTestDirectoryRoot(), "extra-packages");
			if (Directory.Exists(extraPacksDir))
				Directory.Delete(extraPacksDir, true);

			Directory.CreateDirectory(extraPacksDir);

			foreach (var searchPattern in NuGetOnlyPackages)
			{
				// First, try artifacts/ root (CI layout where packages are downloaded directly)
				var packages = Directory.GetFiles(artifactDir, searchPattern).ToList();

				// If not found and running locally, try artifacts/packages/*/Shipping/ (local dotnet cake build layout)
				if (packages.Count == 0 && !TestEnvironment.IsRunningOnCI)
				{
					var packagesDir = Path.Combine(artifactDir, "packages");
					if (Directory.Exists(packagesDir))
					{
						packages = Directory.GetFiles(packagesDir, searchPattern, SearchOption.AllDirectories).ToList();
					}
				}

				// If still not found locally, try .dotnet/library-packs/ (installed workload packages)
				if (packages.Count == 0 && !TestEnvironment.IsRunningOnCI)
				{
					var libraryPacksDir = Path.Combine(mauiDir, ".dotnet", "library-packs");
					if (Directory.Exists(libraryPacksDir))
					{
						packages = Directory.GetFiles(libraryPacksDir, searchPattern).ToList();
					}
				}

				foreach (var pack in packages)
					File.Copy(pack, Path.Combine(extraPacksDir, Path.GetFileName(pack)));
			}

			File.Copy(Path.Combine(TestEnvironment.GetMauiDirectory(), "NuGet.config"), TestNuGetConfig, true);
			FileUtilities.ReplaceInFile(TestNuGetConfig, "<add key=\"nuget-only\" value=\"true\" />", "");
			FileUtilities.ReplaceInFile(TestNuGetConfig, "NUGET_ONLY_PLACEHOLDER", extraPacksDir);
		}

		public void Dispose()
		{
			// One-time teardown if needed
		}
	}

	public abstract class BaseBuildTest : IClassFixture<IntegrationTestFixture>, IDisposable
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

		protected readonly IntegrationTestFixture _fixture;
		protected readonly ITestOutputHelper _output;
		private string? _testName;

		protected BaseBuildTest(IntegrationTestFixture fixture, ITestOutputHelper output)
		{
			_fixture = fixture;
			_output = output;
			
			// Constructor setup (equivalent to [SetUp])
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
				if (_testName != null)
					return _testName;

				// In XUnit, we get test name from the call stack
				var stackTrace = new System.Diagnostics.StackTrace();
				var testMethod = stackTrace.GetFrames()
					.Select(f => f.GetMethod())
					.FirstOrDefault(m => m?.GetCustomAttribute<FactAttribute>() != null || m?.GetCustomAttribute<TheoryAttribute>() != null);

				var result = testMethod?.Name ?? Guid.NewGuid().ToString("N");
				
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
				
				_testName = result;
				return result;
			}
		}

		public string LogDirectory => Path.Combine(TestEnvironment.GetLogDirectory(), TestName);

		public string TestDirectory => Path.Combine(TestEnvironment.GetTestDirectoryRoot(), TestName);

		public string TestNuGetConfig => _fixture.TestNuGetConfig;


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
			// Detailed trimmer warnings, if present
			"TrimmerSingleWarn=false",
			// Allow skipping Xcode version validation via environment variable or TestConfig
			$"ValidateXcodeVersion={!TestEnvironment.SkipXcodeVersionCheck}",
		};

		/// <summary>
		/// Copies log files from the test directory to the artifact publish location.
		/// Uses SYSTEM_JOBATTEMPT environment variable to create separate folders for each retry attempt.
		/// </summary>
		protected void CopyLogsToPublishDirectory()
		{
			try
			{
				// Azure DevOps sets SYSTEM_JOBATTEMPT to indicate which retry attempt this is (1 = first run, 2 = first retry, etc.)
				var jobAttempt = Environment.GetEnvironmentVariable("SYSTEM_JOBATTEMPT") ?? "1";
				var attemptFolder = $"attempt-{jobAttempt}";
				var publishDir = Path.Combine(LogDirectory, attemptFolder);
				
				if (!Directory.Exists(publishDir))
				{
					Directory.CreateDirectory(publishDir);
				}

				if (Directory.Exists(TestDirectory))
				{
					// Copy all log, binlog, and txt files from test directory to publish directory
					var logPatterns = new[] { "*.log", "*.binlog", "*.txt" };
					foreach (var pattern in logPatterns)
					{
						var files = Directory.GetFiles(TestDirectory, pattern, SearchOption.AllDirectories);
						foreach (var file in files)
						{
							try
							{
								var destFile = Path.Combine(publishDir, Path.GetFileName(file));
								// If file with same name exists, add a unique suffix
								if (File.Exists(destFile))
								{
									var nameWithoutExt = Path.GetFileNameWithoutExtension(file);
									var ext = Path.GetExtension(file);
									destFile = Path.Combine(publishDir, $"{nameWithoutExt}_{Guid.NewGuid():N}{ext}");
								}
								File.Copy(file, destFile, overwrite: true);
							}
							catch (Exception ex)
							{
								_output.WriteLine($"Failed to copy log file '{file}': {ex.Message}");
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				_output.WriteLine($"Failed to copy logs to publish directory: {ex.Message}");
			}
		}

		// IDisposable implementation (equivalent to [TearDown])
		public virtual void Dispose()
		{
			// Copy log files to the artifact publish location
			CopyLogsToPublishDirectory();
		}
	}
}
