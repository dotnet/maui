using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Maui.IntegrationTests.Android;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.IntegrationTests
{
	// Use a trait instead of NUnit's Category attribute
	[Trait("Category", Categories.RunOnAndroid)]
	public class AndroidTemplateTests : BaseBuildTest, IDisposable
	{
		private readonly Emulator _testAvd = new Emulator();
		private string _testPackage = "";
		private readonly ITestOutputHelper _outputHelper;

		// Constructor for xUnit dependency injection
		public AndroidTemplateTests(ITestOutputHelper outputHelper) : base(outputHelper)
		{
			_outputHelper = outputHelper;
			
			// xUnit equivalent of OneTimeSetUp
			SetupTestAvd();
		}

		private void SetupTestAvd()
		{
			if (TestEnvironment.IsMacOS && RuntimeInformation.OSArchitecture == Architecture.Arm64)
				_testAvd.Abi = "arm64-v8a";

			Assert.True(_testAvd.AcceptLicenses(out var licenseOutput), $"Failed to accept SDK licenses.\n{licenseOutput}");
			Assert.True(_testAvd.InstallAvd(out var installOutput), $"Failed to install Test AVD.\n{installOutput}");
		}

		 // Made private to avoid xUnit Fact requirement
		private void AndroidTemplateSetUp()
		{
			var emulatorLog = Path.Combine(TestDirectory, $"emulator-launch-{DateTime.UtcNow.ToFileTimeUtc()}.log");
			Assert.True(_testAvd.LaunchAndWaitForAvd(600, emulatorLog), "Failed to launch Test AVD.");
		}
		
		// Test cleanup logic moved to Dispose method
		public override void Dispose()
		{
			// Tear down for individual test
			if (!string.IsNullOrEmpty(_testPackage))
			{
				Adb.UninstallPackage(_testPackage);
			}
			
			// xUnit equivalent of OneTimeTearDown for class fixture
			Adb.KillEmulator(_testAvd.Id);

			// adb.exe can lock certain files on windows, kill it after tests complete
			if (TestEnvironment.IsWindows)
			{
				Adb.Run("kill-server", deviceId: _testAvd.Id);
				foreach (var p in Process.GetProcessesByName("adb.exe"))
					p.Kill();
			}
			
			// Call base class Dispose
			base.Dispose();
		}

		// Replace [TestCase] with [Theory] and [InlineData]
		// Using empty strings instead of null to fix xUnit warnings
		[Theory]
		[InlineData("maui", DotNetPrevious, "Debug", "")]
		[InlineData("maui", DotNetPrevious, "Release", "")]
		[InlineData("maui", DotNetCurrent, "Debug", "")]
		[InlineData("maui", DotNetCurrent, "Release", "")]
		[InlineData("maui", DotNetCurrent, "Release", "full")]
		[InlineData("maui-blazor", DotNetPrevious, "Debug", "")]
		[InlineData("maui-blazor", DotNetPrevious, "Release", "")]
		[InlineData("maui-blazor", DotNetCurrent, "Debug", "")]
		[InlineData("maui-blazor", DotNetCurrent, "Release", "")]
		[InlineData("maui-blazor", DotNetCurrent, "Release", "full")]
		public void RunOnAndroid(string id, string framework, string config, string trimMode)
		{
			AndroidTemplateSetUp(); // Setup the test environment
			
			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.True(DotnetInternal.New(id, projectDir, framework),
				$"Unable to create template {id}. Check test output for errors.");

			var buildProps = BuildProps;
			if (!string.IsNullOrEmpty(trimMode))
			{
				buildProps.Add($"TrimMode={trimMode}");
				buildProps.Add("TrimmerSingleWarn=false");
			}

			AddInstrumentation(projectDir);

			Assert.True(DotnetInternal.Build(projectFile, config, target: "Install", framework: $"{framework}-android", properties: BuildProps),
				$"Project {Path.GetFileName(projectFile)} failed to install. Check test output/attachments for errors.");

			_testPackage = $"com.companyname.{Path.GetFileName(projectDir).ToLowerInvariant()}";
			Assert.True(XHarness.RunAndroid(_testPackage, Path.Combine(projectDir, "xh-results"), -1),
				$"Project {Path.GetFileName(projectFile)} failed to run. Check test output/attachments for errors.");
		}

		void AddInstrumentation(string projectDir)
		{
			var androidDir = Path.Combine(projectDir, "Platforms", "Android");
			var instDestination = Path.Combine(androidDir, "Instrumentation.cs");
			FileUtilities.CreateFileFromResource("TemplateLaunchInstrumentation.cs", instDestination);
			Assert.True(File.Exists(instDestination), "Failed to create Instrumentation.cs");
			FileUtilities.ReplaceInFile(instDestination, "namespace mauitemplate", $"namespace {Path.GetFileName(projectDir)}");

			FileUtilities.ReplaceInFile(Path.Combine(androidDir, "MainActivity.cs"),
				"MainLauncher = true",
				"MainLauncher = true, Name = \"com.microsoft.mauitemplate.MainActivity\"");
		}
	}
}
