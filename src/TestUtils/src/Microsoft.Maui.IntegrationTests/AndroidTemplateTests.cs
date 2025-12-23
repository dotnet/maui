using System.Diagnostics;
using System.Runtime.InteropServices;
using Microsoft.Maui.IntegrationTests.Android;

namespace Microsoft.Maui.IntegrationTests
{
	[Trait("Category", Categories.RunOnAndroid)]
	public class AndroidTemplateTests : BaseBuildTest, IDisposable
	{
		Emulator TestAvd = new Emulator();
		string testPackage = "";

		public AndroidTemplateTests(BuildTestFixture fixture, ITestOutputHelper output) : base(fixture, output)
		{
			if (TestEnvironment.IsMacOS && RuntimeInformation.OSArchitecture == Architecture.Arm64)
				TestAvd.Abi = "arm64-v8a";

			if (!TestAvd.AcceptLicenses(out var licenseOutput))
				throw new Exception($"Failed to accept SDK licenses.\n{licenseOutput}");
			if (!TestAvd.InstallAvd(out var installOutput))
				throw new Exception($"Failed to install Test AVD.\n{installOutput}");
		}

		private void SetupEmulator()
		{
			var emulatorLog = Path.Combine(TestDirectory, $"emulator-launch-{DateTime.UtcNow.ToFileTimeUtc()}.log");
			if (!TestAvd.LaunchAndWaitForAvd(600, emulatorLog))
				throw new Exception("Failed to launch Test AVD.");
		}

		public new void Dispose()
		{
			Adb.UninstallPackage(testPackage);
			Adb.KillEmulator(TestAvd.Id);

			// adb.exe can lock certain files on windows, kill it after tests complete
			if (TestEnvironment.IsWindows)
			{
				Adb.Run("kill-server", deviceId: TestAvd.Id);
				foreach (var p in Process.GetProcessesByName("adb.exe"))
					p.Kill();
			}
			base.Dispose();
		}


		[Theory]
		[InlineData("maui", DotNetPrevious, "Debug", null)]
		[InlineData("maui", DotNetPrevious, "Release", null)]
		[InlineData("maui", DotNetCurrent, "Debug", null)]
		[InlineData("maui", DotNetCurrent, "Release", null)]
		[InlineData("maui", DotNetCurrent, "Release", "full")]
		[InlineData("maui-blazor", DotNetPrevious, "Debug", null)]
		[InlineData("maui-blazor", DotNetPrevious, "Release", null)]
		[InlineData("maui-blazor", DotNetCurrent, "Debug", null)]
		[InlineData("maui-blazor", DotNetCurrent, "Release", null)]
		[InlineData("maui-blazor", DotNetCurrent, "Release", "full")]
		public void RunOnAndroid(string id, string framework, string config, string? trimMode)
		{
			SetupEmulator();

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

			// Write xh-results to the log directory for artifact collection
			var xhResultsDir = Path.Combine(TestEnvironment.GetLogDirectory(), "xh-results", Path.GetFileName(projectDir));
			Directory.CreateDirectory(xhResultsDir);

			testPackage = $"com.companyname.{Path.GetFileName(projectDir).ToLowerInvariant()}";
			Assert.True(XHarness.RunAndroid(testPackage, xhResultsDir, -1),
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
