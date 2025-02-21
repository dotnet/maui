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

		const string s_productName = "Microsoft .NET MAUI";

		const string s_company = "Microsoft";

		const string s_versionPropsFile = "eng/Versions.props";

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
			// Currently we keep the assembly verison at 1.0.0.0
			Assert.AreEqual(1, actual.Major, actual.ToString());
			Assert.AreEqual(0, actual.Minor, actual.ToString());
			Assert.AreEqual(0, actual.Build, actual.ToString());
			Assert.AreEqual(0, actual.Revision, actual.ToString());
		}

		// [Test, TestCaseSource(nameof(references))]
		// public void FileVersion(string assemblyName)
		// {
		// 	Assembly testAssembly = System.Reflection.Assembly.Load(assemblyName);
		// 	FileVersionInfo actual = FileVersionInfo.GetVersionInfo(testAssembly.Location);
		// 	string versionString = GetFileFromRoot(s_versionPropsFile);
		// 	//TODO read MajorVersion from Versions.props
		// 	var xml = new System.Xml.XmlDocument();
		// 	xml.LoadXml(versionString);
		// 	var majorString = xml.SelectSingleNode("//MajorVersion").InnerText;
		// 	var minorString = xml.SelectSingleNode("//MinorVersion").InnerText;
		// 	Version expected = Version.Parse($"{majorString}.{minorString}.0.0");
		// 	Assert.AreEqual(expected.Major, actual.FileMajorPart, $"FileMajorPart is wrong. {actual}");
		// 	Assert.AreEqual(expected.Minor, actual.FileMinorPart, $"FileMinorPart is wrong. {actual}");
		// 	// Fails locally
		// 	//Assert.AreEqual(expected.Build, actual.FileBuildPart, $"FileBuildPart is wrong. {actual.ToString()}");
		// 	//We need to enable this
		// 	//	Assert.AreEqual(ThisAssembly.Git.Commits, version.FilePrivatePart);
		// 	Assert.AreEqual(s_productName, actual.ProductName);
		// 	Assert.AreEqual(s_company, actual.CompanyName);
		// }

		[Test, TestCaseSource(nameof(references))]
		public void ProductAndCompany(string assemblyName)
		{
			Assembly testAssembly = System.Reflection.Assembly.Load(assemblyName);
			FileVersionInfo actual = FileVersionInfo.GetVersionInfo(testAssembly.Location);
			Assert.AreEqual(s_productName, actual.ProductName);
			Assert.AreEqual(s_company, actual.CompanyName);
		}

		internal static string GetFilePathFromRoot(string file)
		{
			var fileFromRoot = IOPath.Combine(TestContext.CurrentContext.TestDirectory, "..", "..", "..", "..", "..", file);
			if (!File.Exists(fileFromRoot))
			{
				//NOTE: VSTS may be running tests in a staging directory, so we can use an environment variable to find the source
				//	https://docs.microsoft.com/en-us/vsts/build-release/concepts/definitions/build/variables?view=vsts&tabs=batch#buildsourcesdirectory
				var sourcesDirectory = Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY");
				if (!string.IsNullOrEmpty(sourcesDirectory))
				{
					fileFromRoot = IOPath.Combine(sourcesDirectory, file);
					if (!File.Exists(fileFromRoot))
					{
						Assert.Fail($"Unable to find {file} at path: {fileFromRoot}");
						return null;
					}
				}
				else
				{
					Assert.Fail($"Unable to find {file} at path: {fileFromRoot}");
					return null;
				}
			}
			return fileFromRoot;
		}

		internal static string GetFileFromRoot(string file)
		{
			var fileFromRootpath = GetFilePathFromRoot(file);
			if (string.IsNullOrEmpty(fileFromRootpath))
			{
				Assert.Fail($"Unable to find {file} at path: {fileFromRootpath}");
				return null;
			}
			return File.ReadAllText(fileFromRootpath);
		}
	}
}
