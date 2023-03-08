

using System.Diagnostics;
using Microsoft.Maui.IntegrationTests.Android;
using Microsoft.VisualStudio.TestPlatform.Utilities;

namespace Microsoft.Maui.IntegrationTests
{
	public class BaseBuildTest
	{
		char[] invalidChars = { '{', '}', '(', ')', '$', ':', ';', '\"', '\'', ',', '=', '.' };

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

		public string TestDirectory => Path.Combine(TestEnvironment.GetTestDirectoryRoot(), TestName);

		// Properties that ensure we don't use cached packages, and *only* the empty NuGet.config
		protected string[] BuildProps = new[]
		{
			"RestoreNoCache=true",
			//"GenerateAppxPackageOnBuild=true",
			$"RestorePackagesPath={Path.Combine(TestEnvironment.GetTestDirectoryRoot(), "packages")}",
			$"RestoreConfigFile={Path.Combine(TestEnvironment.GetMauiDirectory(), "NuGet.config")}",
			// Avoid iOS build warning as error on Windows: There is no available connection to the Mac. Task 'VerifyXcodeVersion' will not be executed
			$"CustomBeforeMicrosoftCSharpTargets={Path.Combine(TestEnvironment.GetMauiDirectory(),"src", "Templates", "TemplateTestExtraTargets.targets")}",
			//Try not restore dependencies of 6.0.10
			$"DisableTransitiveFrameworkReferenceDownloads=true",
		};


		[OneTimeSetUp]
		public void FixtureSetUp() { }

		[SetUp]
		public void SetUp()
		{
			if (Directory.Exists(TestDirectory))
				Directory.Delete(TestDirectory, recursive: true);
		}

		[OneTimeTearDown]
		public void FixtureTearDown() { }

		[TearDown]
		public void TearDown()
		{
			// Clean up test or attach content from failed tests
			if (TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Passed ||
				TestContext.CurrentContext.Result.Outcome.Status == NUnit.Framework.Interfaces.TestStatus.Skipped)
			{
				try
				{
					Directory.Delete(TestDirectory, recursive: true);
				}
				catch (IOException)
				{
				}
			}
			else
			{
				foreach (var log in Directory.GetFiles(Path.Combine(TestDirectory), "*log", SearchOption.AllDirectories))
				{
					TestContext.AddTestAttachment(log, Path.GetFileName(TestDirectory));
				}
			}
		}

	}
}
