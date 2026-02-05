using System.Globalization;
using System.Reflection;

namespace Microsoft.Maui.IntegrationTests
{
	public enum RuntimeVariant
	{
		Mono,
		CoreCLR,
		NativeAOT
	}

	// XUnit class fixture for one-time setup and teardown
	public class IntegrationTestFixture : IDisposable
	{
		// Static lock to ensure only one fixture instance sets up packages at a time
		private static readonly object _setupLock = new object();
		private static bool _isSetupComplete = false;

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
			// Use a lock to ensure only one fixture instance sets up packages at a time
			// This prevents file locking issues on Windows when tests run in parallel
			lock (_setupLock)
			{
				// If setup is already complete, skip it
				if (_isSetupComplete)
					return;

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
				
				// Only delete and recreate if the directory doesn't exist or is empty
				if (!Directory.Exists(extraPacksDir) || Directory.GetFiles(extraPacksDir).Length == 0)
				{
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
						{
							var destPath = Path.Combine(extraPacksDir, Path.GetFileName(pack));
							// Skip if file already exists to avoid file locking issues
							if (!File.Exists(destPath))
								File.Copy(pack, destPath, overwrite: false);
						}
					}
				}

				File.Copy(Path.Combine(TestEnvironment.GetMauiDirectory(), "NuGet.config"), TestNuGetConfig, true);
				FileUtilities.ReplaceInFile(TestNuGetConfig, "<add key=\"nuget-only\" value=\"true\" />", "");
				FileUtilities.ReplaceInFile(TestNuGetConfig, "NUGET_ONLY_PLACEHOLDER", extraPacksDir);

				// Create a Directory.Build.props in the test directory root to prevent MSBuild from
				// walking up and inheriting the MAUI repo's Arcade SDK settings. This ensures test
				// projects use their own local obj/bin folders instead of the repo's artifacts folder.
				var testDirBuildProps = Path.Combine(TestEnvironment.GetTestDirectoryRoot(), "Directory.Build.props");
				if (!File.Exists(testDirBuildProps))
				{
					File.WriteAllText(testDirBuildProps, """
						<Project>
						  <!-- This file stops MSBuild from walking up the directory tree and inheriting
						       the MAUI repo's Directory.Build.props and Arcade SDK settings.
						       This ensures test projects use their own local obj/bin folders. -->
						</Project>
						""");
				}

				// Also create Directory.Build.targets to prevent target inheritance
				var testDirBuildTargets = Path.Combine(TestEnvironment.GetTestDirectoryRoot(), "Directory.Build.targets");
				if (!File.Exists(testDirBuildTargets))
				{
					File.WriteAllText(testDirBuildTargets, """
						<Project>
						  <!-- This file stops MSBuild from walking up the directory tree and inheriting
						       the MAUI repo's Directory.Build.targets. -->
						</Project>
						""");
				}

				_isSetupComplete = true;
			}
		}

		public void Dispose()
		{
			// One-time teardown if needed
		}
	}

	public abstract class BaseBuildTest : IClassFixture<IntegrationTestFixture>, IDisposable
	{
		public const string DotNetCurrent = "net11.0";
		public const string DotNetPrevious = "net10.0";

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
		public const string MauiVersionPrevious = "10.0.30";

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

		/// <summary>
		/// Sets the test identifier based on test parameters.
		/// Should be called at the start of each test method with the test parameters.
		/// </summary>
		protected void SetTestIdentifier(params object?[] parameters)
		{
			if (_testName != null)
				return; // Already set

			// Get method name from stack trace
			var stackTrace = new System.Diagnostics.StackTrace();
			var testMethod = stackTrace.GetFrames()
				.Select(f => f.GetMethod())
				.FirstOrDefault(m => m?.GetCustomAttribute<FactAttribute>() != null || m?.GetCustomAttribute<TheoryAttribute>() != null);

			var methodName = testMethod?.Name ?? "Test";

			// Build identifier from parameters
			var parts = parameters
				.Where(p => p != null)
				.Select(p => p!.ToString()!)
				.Where(s => !string.IsNullOrWhiteSpace(s));

			var result = $"{methodName}_{string.Join("_", parts)}";
			_testName = SanitizeTestName(result);
		}

		private string SanitizeTestName(string name)
		{
			var result = name;

			// Replace invalid characters
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

		public string TestName
		{
			get
			{
				if (_testName != null)
					return _testName;

				// Fallback: If SetTestIdentifier wasn't called, generate from method name + GUID
				var stackTrace = new System.Diagnostics.StackTrace();
				var testMethod = stackTrace.GetFrames()
					.Select(f => f.GetMethod())
					.FirstOrDefault(m => m?.GetCustomAttribute<FactAttribute>() != null || m?.GetCustomAttribute<TheoryAttribute>() != null);

				var methodName = testMethod?.Name ?? "Test";
				var result = $"{methodName}_{Guid.NewGuid():N}";
				_testName = SanitizeTestName(result);
				return _testName;
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
					// Copy all log, binlog, and specific txt files from test directory to publish directory
					var logPatterns = new[] { 
						"*.log", 
						"*.binlog", 
						"acw-map.txt",
						"custom-linker-options*.txt",
						"aot-compiler-path*.txt",
						"customview-map.txt"
					};
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
