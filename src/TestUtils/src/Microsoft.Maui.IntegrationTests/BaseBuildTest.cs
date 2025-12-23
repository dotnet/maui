using System.Globalization;
using System.Runtime.CompilerServices;

namespace Microsoft.Maui.IntegrationTests
{
	public enum RuntimeVariant
	{
		Mono,
		NativeAOT
	}

	[Collection("IntegrationTests")]
	public abstract class BaseBuildTest : IDisposable
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

		protected readonly BuildTestFixture _fixture;
		protected readonly ITestOutputHelper _output;
		private readonly string _testName;

		static readonly char[] invalidChars = { '{', '}', '(', ')', '$', ':', ';', '\"', '\'', ',', '=', '.', '-', ' ', };

		public BaseBuildTest(BuildTestFixture fixture, ITestOutputHelper output, [CallerMemberName] string testName = "")
		{
			_fixture = fixture;
			_output = output;
			_testName = SanitizeTestName(testName);

			// SetUp equivalent: create test directory
			if (Directory.Exists(TestDirectory))
				Directory.Delete(TestDirectory, recursive: true);

			Directory.CreateDirectory(TestDirectory);
		}

		private static string SanitizeTestName(string name)
		{
			var result = name;
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

		public string TestName => _testName;

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
			$"ValidateXcodeVersion={!TestConfig.SkipXcodeVersionCheck}",
		};

		public void Dispose()
		{
			// TearDown equivalent: Attach test content and logs as artifacts
			foreach (var log in Directory.GetFiles(Path.Combine(TestDirectory), "*log", SearchOption.AllDirectories))
			{
				// In xUnit, we write to output instead of using TestContext.AddTestAttachment
				_output.WriteLine($"Log file: {log}");
			}
		}
	}
}
