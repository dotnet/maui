using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Xunit;
using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.MSBuild.UnitTests
{	public class AssemblyInfoTests
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

		[Theory, MemberData(nameof(references))]
		public void AssemblyTitle(string assemblyName)
		{
			Assembly testAssembly = System.Reflection.Assembly.Load(assemblyName);
			Assert.Equal(assemblyName, testAssembly.GetName().Name);
		}

		[Theory, MemberData(nameof(references))]
		public void AssemblyVersion(string assemblyName)
		{
			Assembly testAssembly = System.Reflection.Assembly.Load(assemblyName);
			Version actual = testAssembly.GetName().Version;
			// Currently we keep the assembly verison at 1.0.0.0
			Assert.Equal(1, actual.Major, actual.ToString());
			Assert.Equal(0, actual.Minor, actual.ToString());
			Assert.Equal(0, actual.Build, actual.ToString());
			Assert.Equal(0, actual.Revision, actual.ToString());
		}

		// [Theory, MemberData(nameof(references))]
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
		// 	Assert.Equal(expected.Major, actual.FileMajorPart, $"FileMajorPart is wrong. {actual}");
		// 	Assert.Equal(expected.Minor, actual.FileMinorPart, $"FileMinorPart is wrong. {actual}");
		// 	// Fails locally
		// 	//Assert.Equal(expected.Build, actual.FileBuildPart, $"FileBuildPart is wrong. {actual.ToString()}");
		// 	//We need to enable this
		// 	//	Assert.Equal(ThisAssembly.Git.Commits, version.FilePrivatePart);
		// 	Assert.Equal(s_productName, actual.ProductName);
		// 	Assert.Equal(s_company, actual.CompanyName);
		// }

		[Theory, MemberData(nameof(references))]
		public void ProductAndCompany(string assemblyName)
		{
			Assembly testAssembly = System.Reflection.Assembly.Load(assemblyName);
			FileVersionInfo actual = FileVersionInfo.GetVersionInfo(testAssembly.Location);
			Assert.Equal(s_productName, actual.ProductName);
			Assert.Equal(s_company, actual.CompanyName);
		}

		internal static string GetFilePathFromRoot(string file)
		{
			var fileFromRoot = IOPath.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", file);
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
						throw new Xunit.Sdk.XunitException($"Unable to find {file} at path: {fileFromRoot}");
						return null;
					}
				}
				else
				{
					throw new Xunit.Sdk.XunitException($"Unable to find {file} at path: {fileFromRoot}");
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
				throw new Xunit.Sdk.XunitException($"Unable to find {file} at path: {fileFromRootpath}");
				return null;
			}
			return File.ReadAllText(fileFromRootpath);
		}
	}
}
