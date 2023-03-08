using System.Diagnostics;
using Microsoft.Maui.IntegrationTests.Android;
using NUnit.Framework.Interfaces;

namespace Microsoft.Maui.IntegrationTests
{
	public class TemplateRunTests : BaseBuildTest
	{
		Emulator TestAvd = new Emulator();

		[OneTimeSetUp]
		public void TemplateRunFixtureSetUp()
		{
			Assert.IsTrue(TestAvd.LaunchAndWaitForAvd(720), "Failed to launch Test AVD.");
		}

		[OneTimeTearDown]
		public void TemplateRunFixtureTear()
		{
			Adb.KillEmulator(TestAvd.Id);

			// adb.exe can lock certain files on windows, kill it after tests complete
			if (TestEnvironment.IsWindows)
			{
				Adb.Run("kill-server", out _);
				foreach (var p in Process.GetProcessesByName("adb.exe"))
					p.Kill();
			}
		}

		[Test]
		[TestCase("maui", "Debug", "net7.0")]
		//[TestCase("maui", "Release", "net7.0")]
		//[TestCase("maui-blazor", "Debug", "net7.0")]
		//[TestCase("maui-blazor", "Release", "net7.0")]
		public void RunOnAndroid(string id, string config, string framework)
		{
			var projectDir = TestDirectory;
			var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

			Assert.IsTrue(DotnetInternal.New(id, projectDir, framework),
				$"Unable to create template {id}. Check test output for errors.");

			AddInstrumentation(projectDir);

			Assert.IsTrue(DotnetInternal.Build(projectFile, config, target: "Install", framework: $"{framework}-android", properties: BuildProps),
				$"Project {Path.GetFileName(projectFile)} failed to install. Check test output/attachments for errors.");

			Assert.IsTrue(XHarness.RunAndroid($"com.companyname.{Path.GetFileName(projectDir).ToLower()}", Path.Combine(projectDir, "xh-results"), -1),
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

		[Test]
		[TestCase("maui", "Debug", "net7.0")]
		[TestCase("maui", "Release", "net7.0")]
		[TestCase("maui-blazor", "Debug", "net7.0")]
		[TestCase("maui-blazor", "Release", "net7.0")]
		public void RunOniOS(string id, string config, string framework)
		{
			if (!TestEnvironment.IsMacOS)
				Assert.Ignore("iOS run template tests only run on macOS.");
		}

	}
}