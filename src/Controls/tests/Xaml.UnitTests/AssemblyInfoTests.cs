using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using Xunit;
using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.MSBuild.UnitTests
{
	[Collection("Xaml Inflation")]
	public class AssemblyInfoTests
	{
		public static TheoryData<string> References => new TheoryData<string>
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

		[Theory]
		[MemberData(nameof(References))]
		public void AssemblyTitle(string assemblyName)
		{
			Assembly testAssembly = System.Reflection.Assembly.Load(assemblyName);
			Assert.Equal(assemblyName, testAssembly.GetName().Name);
		}

		[Theory]
		[MemberData(nameof(References))]
		public void AssemblyVersion(string assemblyName)
		{
			Assembly testAssembly = System.Reflection.Assembly.Load(assemblyName);
			Version actual = testAssembly.GetName().Version;
			// Currently we keep the assembly verison at 10.0.0.0
			Assert.Equal(11, actual.Major);
			Assert.Equal(0, actual.Minor);
			Assert.Equal(0, actual.Build);
			Assert.Equal(0, actual.Revision);
		}

		// [Theory]
		// [MemberData(nameof(References))]
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
		// 	Assert.Equal(expected.Major, actual.FileMajorPart);
		// 	Assert.Equal(expected.Minor, actual.FileMinorPart);
		// 	// Fails locally
		// 	//Assert.Equal(expected.Build, actual.FileBuildPart);
		// 	//We need to enable this
		// 	//	Assert.Equal(ThisAssembly.Git.Commits, version.FilePrivatePart);
		// 	Assert.Equal(s_productName, actual.ProductName);
		// 	Assert.Equal(s_company, actual.CompanyName);
		// }

		[Theory]
		[MemberData(nameof(References))]
		public void ProductAndCompany(string assemblyName)
		{
			Assembly testAssembly = System.Reflection.Assembly.Load(assemblyName);
			FileVersionInfo actual = FileVersionInfo.GetVersionInfo(testAssembly.Location);
			Assert.Equal(s_productName, actual.ProductName);
			Assert.Equal(s_company, actual.CompanyName);
		}

		internal static string GetFilePathFromRoot(string file)
		{
			// First, check Helix correlation payload (HELIX_CORRELATION_PAYLOAD environment variable)
			var helixPayload = Environment.GetEnvironmentVariable("HELIX_CORRELATION_PAYLOAD");
			if (!string.IsNullOrEmpty(helixPayload))
			{
				var normalizedFile = file.Replace('\\', '/');
				
				// The MSBuild test files are copied to msbuild/ in the Helix payload
				// Map paths like "src/Controls/tests/Xaml.UnitTests/MSBuild/X" to "msbuild/X"
				var msbuildPrefix = "src/Controls/tests/Xaml.UnitTests/MSBuild/";
				if (normalizedFile.StartsWith(msbuildPrefix, StringComparison.OrdinalIgnoreCase))
				{
					var relativePath = normalizedFile.Substring(msbuildPrefix.Length);
					var helixPath = IOPath.Combine(helixPayload, "msbuild", relativePath);
					if (File.Exists(helixPath))
						return helixPath;
				}
				
				// Files from src/ folder are in the "src" correlation payload
				// Map paths like "src/Controls/src/Build.Tasks/nuget/..." directly to "src/..." in payload
				if (normalizedFile.StartsWith("src/", StringComparison.OrdinalIgnoreCase))
				{
					var helixPath = IOPath.Combine(helixPayload, normalizedFile);
					if (File.Exists(helixPath))
						return helixPath;
				}
			}

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
						Assert.Fail($"Unable to find {file} at path: {fileFromRoot}");
						return string.Empty;
					}
				}
				else
				{
					Assert.Fail($"Unable to find {file} at path: {fileFromRoot}");
					return string.Empty;
				}
			}
			return fileFromRoot;
		}

		/// <summary>
		/// Returns true if running on Helix (any Helix environment)
		/// </summary>
		internal static bool IsRunningOnHelix()
		{
			var helixPayload = Environment.GetEnvironmentVariable("HELIX_CORRELATION_PAYLOAD");
			return !string.IsNullOrEmpty(helixPayload);
		}

		/// <summary>
		/// Find the repo root by looking for Microsoft.Maui.sln
		/// </summary>
		static string GetTopDirRecursive(string searchDirectory, int maxSearchDepth = 7)
		{
			if (string.IsNullOrEmpty(searchDirectory))
				return null;
			
			if (File.Exists(IOPath.Combine(searchDirectory, "Microsoft.Maui.sln")))
				return searchDirectory;

			if (maxSearchDepth <= 0)
				return null;

			var parent = Directory.GetParent(searchDirectory);
			if (parent == null)
				return null;
				
			return GetTopDirRecursive(parent.FullName, --maxSearchDepth);
		}

		/// <summary>
		/// Gets the path to the MSBuild test files, checking both local repo and Helix payload locations.
		/// </summary>
		internal static string GetMSBuildTestsPath()
		{
			// First, check Helix correlation payload (HELIX_CORRELATION_PAYLOAD environment variable)
			var helixPayload = Environment.GetEnvironmentVariable("HELIX_CORRELATION_PAYLOAD");
			if (!string.IsNullOrEmpty(helixPayload))
			{
				var helixPath = IOPath.Combine(helixPayload, "msbuild", "_Directory.Build.props");
				if (File.Exists(helixPath))
					return IOPath.GetDirectoryName(helixPath);
			}

			// Check relative path from AppContext.BaseDirectory
			var fileFromRoot = IOPath.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "Controls", "tests", "Xaml.UnitTests", "MSBuild", "_Directory.Build.props");
			if (File.Exists(fileFromRoot))
				return IOPath.GetDirectoryName(fileFromRoot);

			// Check BUILD_SOURCESDIRECTORY environment variable (Azure DevOps)
			var sourcesDirectory = Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY");
			if (!string.IsNullOrEmpty(sourcesDirectory))
			{
				fileFromRoot = IOPath.Combine(sourcesDirectory, "src", "Controls", "tests", "Xaml.UnitTests", "MSBuild", "_Directory.Build.props");
				if (File.Exists(fileFromRoot))
					return IOPath.GetDirectoryName(fileFromRoot);
			}
			return null;
		}


		internal static string GetFileFromRoot(string file)
		{
			var fileFromRootpath = GetFilePathFromRoot(file);
			if (string.IsNullOrEmpty(fileFromRootpath))
			{
				Assert.Fail($"Unable to find {file} at path: {fileFromRootpath}");
				return string.Empty;
			}
			return File.ReadAllText(fileFromRootpath);
		}
	}
}
