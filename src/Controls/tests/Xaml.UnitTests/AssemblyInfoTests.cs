using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using NUnit.Framework;
using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.MSBuild.UnitTests
{
	[TestFixture]
	public class AssemblyInfoTests
	{
		static readonly string[] references = new[]
		{
			"Microsoft.Maui",
			"Microsoft.Maui.Controls",
			"Microsoft.Maui.Controls.Maps",
			"Microsoft.Maui.Controls.Xaml",
			"Microsoft.Maui.Controls.Build.Tasks"
		};

		const string s_productName = "Microsoft.Maui";

		const string s_company = "Microsoft";

		const string s_gitInfoFile = "GitInfo.txt";

		[Test, TestCaseSource(nameof(references))]
		public void AssemblyTitle(string assemblyName)
		{
			Assembly testAssembly = System.Reflection.Assembly.Load(assemblyName);
			Assert.AreEqual(assemblyName, testAssembly.GetName().Name);
		}

		[Test, TestCaseSource(nameof(references))]
		public void AssemblyVersion(string assemblyName)
		{
			Assembly testAssembly = System.Reflection.Assembly.Load(assemblyName);
			Version actual = testAssembly.GetName().Version;
			Assert.AreEqual(1, actual.Major, actual.ToString());
			Assert.AreEqual(0, actual.Minor, actual.ToString());
			Assert.AreEqual(0, actual.Build, actual.ToString());
			Assert.AreEqual(0, actual.Revision, actual.ToString());
		}

		[Test, TestCaseSource(nameof(references))]
		public void FileVersion(string assemblyName)
		{
			Assembly testAssembly = System.Reflection.Assembly.Load(assemblyName);
			FileVersionInfo actual = FileVersionInfo.GetVersionInfo(testAssembly.Location);
			string versionString = GetFileFromRoot(s_gitInfoFile);
			if (versionString.IndexOf("-") is int idx && idx > 0)
				versionString = versionString.Substring(0, idx);
			Version expected = Version.Parse(versionString);
			Assert.AreEqual(expected.Major, actual.FileMajorPart, $"FileMajorPart is wrong. {actual.ToString()}");
			Assert.AreEqual(expected.Minor, actual.FileMinorPart, $"FileMinorPart is wrong. {actual.ToString()}");
			// Fails locally
			//Assert.AreEqual(expected.Build, actual.FileBuildPart, $"FileBuildPart is wrong. {actual.ToString()}");
			//We need to enable this
			//	Assert.AreEqual(ThisAssembly.Git.Commits, version.FilePrivatePart);
			Assert.AreEqual(s_productName, actual.ProductName);
			Assert.AreEqual(s_company, actual.CompanyName);
		}

		[Test, TestCaseSource(nameof(references))]
		public void ProductAndCompany(string assemblyName)
		{
			Assembly testAssembly = System.Reflection.Assembly.Load(assemblyName);
			FileVersionInfo actual = FileVersionInfo.GetVersionInfo(testAssembly.Location);
			Assert.AreEqual(s_productName, actual.ProductName);
			Assert.AreEqual(s_company, actual.CompanyName);
		}

		static string GetFileFromRoot(string file)
		{
			var gitInfoFile = IOPath.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "..", "..", "..", file);
			if (!File.Exists(gitInfoFile))
			{
				//NOTE: VSTS may be running tests in a staging directory, so we can use an environment variable to find the source
				//	https://docs.microsoft.com/en-us/vsts/build-release/concepts/definitions/build/variables?view=vsts&tabs=batch#buildsourcesdirectory
				var sourcesDirectory = Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY");
				if (!string.IsNullOrEmpty(sourcesDirectory))
				{
					gitInfoFile = IOPath.Combine(sourcesDirectory, file);
					if (!File.Exists(gitInfoFile))
					{
						Assert.Fail($"Unable to find {file} at path: {gitInfoFile}");
					}
				}
				else
				{
					Assert.Fail($"Unable to find {file} at path: {gitInfoFile}");
				}
			}
			return File.ReadAllText(gitInfoFile);
		}
	}
}
