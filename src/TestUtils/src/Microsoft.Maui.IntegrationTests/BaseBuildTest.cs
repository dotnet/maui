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
		/// Copy NuGet packages that are not installed as part of the workload and set up NuGet.config
		/// See: `PrepareSeparateBuildContext` in `eng/cake/dotnet.cake`.
		/// </summary>
		/// <exception cref="DirectoryNotFoundException"></exception>
		[OneTimeSetUp]
		public void BuildTestFxtSetUp()
		{
			// Log diagnostic information about test environment
			LogTestEnvironmentInfo();

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
			// Log all files in test directory and log directory for debugging
			LogTestDirectoryContents();
			LogLogDirectoryContents();
			
			// Copy log files to the artifact publish location
			CopyLogsToPublishDirectory();

			// Attach test content and logs as artifacts
			try
			{
				if (Directory.Exists(TestDirectory))
				{
					var logFiles = Directory.GetFiles(TestDirectory, "*log", SearchOption.AllDirectories);
					TestContext.WriteLine($"[TearDown] Found {logFiles.Length} log files to attach in {TestDirectory}");
					foreach (var log in logFiles)
					{
						TestContext.WriteLine($"[TearDown] Attaching: {log}");
						TestContext.AddTestAttachment(log, Path.GetFileName(TestDirectory));
					}
				}
				else
				{
					TestContext.WriteLine($"[TearDown] WARNING: TestDirectory does not exist: {TestDirectory}");
				}
			}
			catch (Exception ex)
			{
				TestContext.WriteLine($"[TearDown] ERROR attaching logs: {ex.Message}");
			}
		}

		/// <summary>
		/// Copies log files from the test directory to the artifact publish location.
		/// </summary>
		protected void CopyLogsToPublishDirectory()
		{
			try
			{
				var publishDir = LogDirectory;
				TestContext.WriteLine($"[CopyLogs] Target publish directory: {publishDir}");
				
				if (!Directory.Exists(publishDir))
				{
					Directory.CreateDirectory(publishDir);
					TestContext.WriteLine($"[CopyLogs] Created directory: {publishDir}");
				}

				if (Directory.Exists(TestDirectory))
				{
					// Copy all log, binlog, and txt files from test directory to publish directory
					var logPatterns = new[] { "*.log", "*.binlog", "*.txt" };
					foreach (var pattern in logPatterns)
					{
						var files = Directory.GetFiles(TestDirectory, pattern, SearchOption.AllDirectories);
						TestContext.WriteLine($"[CopyLogs] Found {files.Length} {pattern} files to copy");
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
								TestContext.WriteLine($"[CopyLogs] Copied: {file} -> {destFile}");
							}
							catch (Exception ex)
							{
								TestContext.WriteLine($"[CopyLogs] ERROR copying {file}: {ex.Message}");
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				TestContext.WriteLine($"[CopyLogs] ERROR: {ex.Message}");
			}
		}

		/// <summary>
		/// Logs diagnostic information about the test environment at the start of the test fixture.
		/// </summary>
		protected void LogTestEnvironmentInfo()
		{
			TestContext.WriteLine("=== Test Environment Info ===");
			TestContext.WriteLine($"[ENV] MAUI Directory: {TestEnvironment.GetMauiDirectory()}");
			TestContext.WriteLine($"[ENV] Log Directory: {TestEnvironment.GetLogDirectory()}");
			TestContext.WriteLine($"[ENV] Test Directory Root: {TestEnvironment.GetTestDirectoryRoot()}");
			TestContext.WriteLine($"[ENV] Is Running on CI: {TestEnvironment.IsRunningOnCI}");
			TestContext.WriteLine($"[ENV] Is macOS: {TestEnvironment.IsMacOS}");
			TestContext.WriteLine($"[ENV] Is ARM64: {TestEnvironment.IsArm64}");
			TestContext.WriteLine($"[ENV] iOS Simulator RID: {TestEnvironment.IOSSimulatorRuntimeIdentifier}");
			TestContext.WriteLine($"[ENV] AGENT_TEMPDIRECTORY: {Environment.GetEnvironmentVariable("AGENT_TEMPDIRECTORY") ?? "(not set)"}");
			TestContext.WriteLine($"[ENV] BUILD_ARTIFACTSTAGINGDIRECTORY: {Environment.GetEnvironmentVariable("BUILD_ARTIFACTSTAGINGDIRECTORY") ?? "(not set)"}");
			TestContext.WriteLine($"[ENV] LogDirectory env var: {Environment.GetEnvironmentVariable("LogDirectory") ?? "(not set)"}");
			TestContext.WriteLine("=== End Environment Info ===");
		}

		/// <summary>
		/// Logs all files in the test directory for debugging purposes.
		/// </summary>
		protected void LogTestDirectoryContents()
		{
			TestContext.WriteLine($"=== Test Directory Contents: {TestDirectory} ===");
			try
			{
				if (!Directory.Exists(TestDirectory))
				{
					TestContext.WriteLine($"[WARN] Directory does not exist: {TestDirectory}");
					return;
				}

				// List all files recursively
				var allFiles = Directory.GetFiles(TestDirectory, "*", SearchOption.AllDirectories);
				TestContext.WriteLine($"[DIR] Total files: {allFiles.Length}");
				
				foreach (var file in allFiles.Take(50)) // Limit to 50 files to avoid log spam
				{
					var info = new FileInfo(file);
					TestContext.WriteLine($"[FILE] {file} ({info.Length} bytes)");
				}

				if (allFiles.Length > 50)
				{
					TestContext.WriteLine($"[DIR] ... and {allFiles.Length - 50} more files");
				}

				// List log files specifically
				var logFiles = allFiles.Where(f => f.EndsWith(".log", StringComparison.OrdinalIgnoreCase) || 
					f.EndsWith(".binlog", StringComparison.OrdinalIgnoreCase) ||
					f.EndsWith(".txt", StringComparison.OrdinalIgnoreCase)).ToArray();
				TestContext.WriteLine($"[DIR] Log/txt/binlog files: {logFiles.Length}");
				foreach (var log in logFiles)
				{
					TestContext.WriteLine($"[LOG] {log}");
				}
			}
			catch (Exception ex)
			{
				TestContext.WriteLine($"[ERROR] Failed to list directory contents: {ex.Message}");
			}
			TestContext.WriteLine("=== End Test Directory Contents ===");
		}

		/// <summary>
		/// Logs all files in the log directory for debugging purposes.
		/// </summary>
		protected void LogLogDirectoryContents()
		{
			var logDir = LogDirectory;
			TestContext.WriteLine($"=== Log Directory Contents: {logDir} ===");
			try
			{
				if (!Directory.Exists(logDir))
				{
					TestContext.WriteLine($"[WARN] Log directory does not exist: {logDir}");
					return;
				}

				var allFiles = Directory.GetFiles(logDir, "*", SearchOption.AllDirectories);
				TestContext.WriteLine($"[DIR] Total files in log directory: {allFiles.Length}");
				foreach (var file in allFiles)
				{
					var info = new FileInfo(file);
					TestContext.WriteLine($"[FILE] {file} ({info.Length} bytes)");
				}
			}
			catch (Exception ex)
			{
				TestContext.WriteLine($"[ERROR] Failed to list log directory contents: {ex.Message}");
			}
			TestContext.WriteLine("=== End Log Directory Contents ===");
		}

	}
}
