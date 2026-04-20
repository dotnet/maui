using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using Mono.Cecil;
using Xunit;
using Xunit.Abstractions;
using static Microsoft.Maui.Controls.MSBuild.UnitTests.MSBuildXmlExtensions;
using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.MSBuild.UnitTests
{
	//This set of tests is for validating Microsoft.Maui.Controls.targets
	[Trait("Category", "LongRunning")]
	public class MSBuildTests : IDisposable
	{
		static readonly string[] references = {
			"Microsoft.Maui.Controls.dll",
			"Microsoft.Maui.Controls.Xaml.dll",
			"Microsoft.Maui.dll",
		};

		class Xaml
		{
			const string MicrosoftMauiControlsFormsDefaultNamespace = "http://schemas.microsoft.com/dotnet/2021/maui";
			const string MicrosoftMauiControlsFormsXNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

			public static readonly string MainPage = $@"
				<ContentPage
					xmlns=""{MicrosoftMauiControlsFormsDefaultNamespace}""
					xmlns:x=""{MicrosoftMauiControlsFormsXNamespace}""
					x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.MainPage"">
					<Label x:Name=""label0""/>
				</ContentPage>";

			public static readonly string CustomView = $@"
				<ContentView
					xmlns=""{MicrosoftMauiControlsFormsDefaultNamespace}""
					xmlns:x=""{MicrosoftMauiControlsFormsXNamespace}""
					x:Class=""Microsoft.Maui.Controls.Xaml.UnitTests.CustomView"">
					<Label x:Name=""label0""/>
				</ContentView>";
		}

		class Css
		{
			public const string Foo = @"
				label {
					color: azure;
					background-color: aliceblue;
				}";
		}

		static string GetTfm()
		{
			// Returns something like `.NET 6.0.1`
			var fd = System.Runtime.InteropServices.RuntimeInformation.FrameworkDescription;
			if (Version.TryParse(System.Text.RegularExpressions.Regex.Match(fd, @"\d+\.\d+\.\d+")?.Value, out var version))
				return $"net{version.Major}.{version.Minor}";
			return "net7.0";
		}

		readonly string testDirectory;
		string tempDirectory;
		string intermediateDirectory;
		readonly ITestOutputHelper _output;

		public MSBuildTests(ITestOutputHelper output)
		{
			_output = output;
			testDirectory = IOPath.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
		}

		void SetUp([CallerMemberName] string testName = null)
		{
			// Sanitize test name for use in path
			var sanitizedName = testName
				.Replace('"', '_')
				.Replace('(', '_')
				.Replace(')', '_');

			tempDirectory = IOPath.Combine(testDirectory, "temp", sanitizedName);
			intermediateDirectory = IOPath.Combine(tempDirectory, "obj", "Debug", GetTfm());
			Directory.CreateDirectory(tempDirectory);

			// Find the Directory.Build files - they handle both local and Helix via MSBuild conditions
			string props, targets;
			if (AssemblyInfoTests.IsRunningOnHelix())
			{
				// On Helix, the test DLL directory contains the MSBuild folder with our files
				var msbuildDir = IOPath.Combine(testDirectory, "MSBuild");
				props = IOPath.Combine(msbuildDir, "_Directory.Build.props");
				targets = IOPath.Combine(msbuildDir, "_Directory.Build.targets");
			}
			else
			{
				// Local development - find from repo root
				props = AssemblyInfoTests.GetFilePathFromRoot(IOPath.Combine("src", "Controls", "tests", "Xaml.UnitTests", "MSBuild", "_Directory.Build.props"));
				targets = AssemblyInfoTests.GetFilePathFromRoot(IOPath.Combine("src", "Controls", "tests", "Xaml.UnitTests", "MSBuild", "_Directory.Build.targets"));
			}

			if (!File.Exists(props))
			{
				//NOTE: VSTS may be running tests in a staging directory, so we can use an environment variable to find the source
				//https://docs.microsoft.com/en-us/vsts/build-release/concepts/definitions/build/variables?view=vsts&tabs=batch#buildsourcesdirectory
				throw new FileNotFoundException($"Unable to find _Directory.Build.props at path: {props}");
			}

			File.Copy(props, IOPath.Combine(tempDirectory, "Directory.Build.props"), true);
			File.Copy(targets, IOPath.Combine(tempDirectory, "Directory.Build.targets"), true);
		}

		public void Dispose()
		{
			// Note: xUnit doesn't provide a way to check if the test failed, so we always clean up
			if (tempDirectory == null)
				return;

			//NOTE: Windows can throw IOException: The process cannot access the file XYZ because it is being used by another process.
			//A simple retry-and-give-up approach should be good enough
			for (int i = 0; i < 3; i++)
			{
				try
				{
					if (Directory.Exists(tempDirectory))
					{
						Directory.Delete(tempDirectory, true);
					}
					break; //Success
				}
				catch (IOException)
				{
					System.Threading.Thread.Sleep(100);
				}
			}
		}

		/// <summary>
		/// Creates a base csproj file for these unit tests
		/// </summary>
		XElement NewProject()
		{
			var project = NewElement("Project");

			var propertyGroup = NewElement("PropertyGroup");
			project.WithAttribute("Sdk", "Microsoft.NET.Sdk");
			propertyGroup.Add(NewElement("TargetFramework").WithValue(GetTfm()));
			//NOTE: we don't want SDK-style projects to auto-add files, tests should be able to control this
			propertyGroup.Add(NewElement("EnableDefaultCompileItems").WithValue("False"));
			propertyGroup.Add(NewElement("EnableDefaultEmbeddedResourceItems").WithValue("False"));
			project.Add(propertyGroup);

			var itemGroup = NewElement("ItemGroup");
			foreach (var assembly in references)
			{
				var reference = NewElement("Reference").WithAttribute("Include", assembly);
				if (assembly.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
				{
					reference.Add(NewElement("HintPath").WithValue(IOPath.Combine("..", "..", assembly)));
				}
				itemGroup.Add(reference);
			}
			project.Add(itemGroup);

			//Let's enable XamlC assembly-wide
			project.Add(AddFile("AssemblyInfo.cs", "Compile", "[assembly: Microsoft.Maui.Controls.Xaml.XamlCompilation (Microsoft.Maui.Controls.Xaml.XamlCompilationOptions.Compile)]"));

			//Add a single CSS file
			project.Add(AddFile("Foo.css", "MauiCss", Css.Foo));

			return project;
		}

		XElement AddFile(string name, string buildAction, string contents)
		{
			var filePath = IOPath.Combine(tempDirectory, name.Replace('\\', IOPath.DirectorySeparatorChar).Replace('/', IOPath.DirectorySeparatorChar));
			Directory.CreateDirectory(IOPath.GetDirectoryName(filePath));
			File.WriteAllText(filePath, contents);
			var itemGroup = NewElement("ItemGroup");
			itemGroup.Add(NewElement(buildAction).WithAttribute("Include", name));
			return itemGroup;
		}

		void WriteFile(string name, string contents)
		{
			var filePath = IOPath.Combine(tempDirectory, name.Replace('\\', IOPath.DirectorySeparatorChar).Replace('/', IOPath.DirectorySeparatorChar));
			Directory.CreateDirectory(IOPath.GetDirectoryName(filePath));
			File.WriteAllText(filePath, contents);
		}

		string Build(string projectFile, string target = "Build", string verbosity = "normal", string additionalArgs = "", bool shouldSucceed = true)
		{
			var builder = new StringBuilder();
			void onData(object s, DataReceivedEventArgs e)
			{
				lock (builder)
					if (e.Data != null)
					{
						builder.AppendLine(e.Data);
						_output.WriteLine(e.Data);
					}
			}
			;

			var ext = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "";
			var dotnet = IOPath.Combine(testDirectory, "..", "..", "..", "..", "..", "..", "..", "bin", "dotnet", $"dotnet{ext}");
			if (!File.Exists(dotnet))
			{
				_output.WriteLine($"Using 'dotnet', did not find: {dotnet}");

				// If we don't have .\bin\dotnet\dotnet, try the system one
				dotnet = "dotnet";
			}
			else
			{
				dotnet = IOPath.GetFullPath(dotnet);

				_output.WriteLine($"Using '{dotnet}'");
			}

			var psi = new ProcessStartInfo
			{
				FileName = dotnet,
				Arguments = $"build -v:{verbosity} -nologo {projectFile} -t:{target} -bl {additionalArgs}",
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				WorkingDirectory = tempDirectory,
			};
			using (var p = new Process { StartInfo = psi })
			{
				p.ErrorDataReceived += onData;
				p.OutputDataReceived += onData;

				p.Start();
				p.BeginErrorReadLine();
				p.BeginOutputReadLine();
				p.WaitForExit();

				// Log the binlog location (xUnit doesn't have test attachments like NUnit)
				var binlog = IOPath.Combine(IOPath.GetDirectoryName(projectFile), "msbuild.binlog");
				if (File.Exists(binlog))
				{
					_output.WriteLine($"MSBuild binlog: {binlog}");
				}

				if (shouldSucceed)
					Assert.Equal(0, p.ExitCode);
				else
					Assert.NotEqual(0, p.ExitCode);

				return builder.ToString();
			}
		}

		void AssertExists(string path, bool nonEmpty = false)
		{
			Assert.True(File.Exists(path), $"{path} should exist!");

			if (nonEmpty)
				Assert.True(new FileInfo(path).Length > 0, $"{path} is empty!");
		}

		void AssertDoesNotExist(string path)
		{
			Assert.False(File.Exists(path), $"{path} should *not* exist!");
		}

		void AssertTypeExists(string assemblyPath, string fullTypeName)
		{
			using var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
			Assert.Contains(assembly.MainModule.Types.Select(t => t.FullName), t => t == fullTypeName);
		}

		void AssertTypeDoesNotExist(string assemblyPath, string fullTypeName)
		{
			using var assembly = AssemblyDefinition.ReadAssembly(assemblyPath);
			Assert.DoesNotContain(assembly.MainModule.Types.Select(t => t.FullName), t => t == fullTypeName);
		}

		void AddSingleProjectBeforeTargetsImport(XElement project)
		{
			var beforeTargetsPath = AssemblyInfoTests.GetFilePathFromRoot(IOPath.Combine("src", "Controls", "src", "Build.Tasks", "nuget", "buildTransitive", "netstandard2.0", "Microsoft.Maui.Controls.SingleProject.Before.targets"));
			project.Add(NewElement("Import").WithAttribute("Project", beforeTargetsPath));
		}

		void AddSingleProjectTargetsImport(XElement project)
		{
			var targetsPath = AssemblyInfoTests.GetFilePathFromRoot(IOPath.Combine("src", "Controls", "src", "Build.Tasks", "nuget", "buildTransitive", "netstandard2.0", "Microsoft.Maui.Controls.SingleProject.targets"));
			project.Add(NewElement("Import").WithAttribute("Project", targetsPath));
		}

		void AddMauiReferences(XElement project)
		{
			var itemGroup = NewElement("ItemGroup");
			foreach (var assembly in references)
			{
				var reference = NewElement("Reference").WithAttribute("Include", assembly);
				if (assembly.EndsWith(".dll", StringComparison.OrdinalIgnoreCase))
				{
					reference.Add(NewElement("HintPath").WithValue(IOPath.Combine("..", "..", assembly)));
				}
				itemGroup.Add(reference);
			}

			project.Add(itemGroup);
		}

		[Fact]
		public void BuildAProject()
		{
			SetUp();
			var project = NewProject();
			project.Add(AddFile("MainPage.xaml", "MauiXaml", Xaml.MainPage));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			Build(projectFile);

			AssertExists(IOPath.Combine(intermediateDirectory, "test.dll"), nonEmpty: true);
			AssertExists(IOPath.Combine(intermediateDirectory, "XamlC.stamp"));
		}

		[Theory]
		[InlineData("Debug")]
		[InlineData("Release")]
		public void HotReloadSupportForXSG(string configuration)
		{
			SetUp();
			var project = NewProject();
			project.Add(AddFile("MainPage.xaml", "MauiXaml", Xaml.MainPage));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			Build(projectFile, additionalArgs: $"-c {configuration} -p:MauiXamlInflator=SourceGen -p:EmitCompilerGeneratedFiles=True -p:CompilerGeneratedFilesOutputPath=Generated");

			var generatorDirectory = IOPath.Combine(tempDirectory, "Generated", "Microsoft.Maui.Controls.SourceGen", "Microsoft.Maui.Controls.SourceGen.XamlGenerator");
			AssertExists(IOPath.Combine(generatorDirectory, "MainPage.xaml.sg.cs"), nonEmpty: true);
			AssertExists(IOPath.Combine(generatorDirectory, "MainPage.xaml.xsg.cs"), nonEmpty: true);
			
			var sg = File.ReadAllText(IOPath.Combine(generatorDirectory, "MainPage.xaml.sg.cs"));
			var xsg = File.ReadAllText(IOPath.Combine(generatorDirectory, "MainPage.xaml.xsg.cs"));
			if (configuration == "Debug")
			{
				Assert.Contains("InitializeComponentRuntime", sg, StringComparison.Ordinal);
				Assert.Contains("InitializeComponentRuntime", xsg, StringComparison.Ordinal);
			}
			else
            {
				Assert.DoesNotContain("InitializeComponentRuntime", sg, StringComparison.Ordinal);
				Assert.DoesNotContain("InitializeComponentRuntime", xsg, StringComparison.Ordinal);
            }

		}

		// Tests the MauiXamlCValidateOnly=True MSBuild property
		[Theory]
		[InlineData("Debug")]
		[InlineData("Release")]
		[InlineData("ReleaseProd")]
		public void ValidateOnly(string configuration)
		{
			SetUp();
			var project = NewProject();
			project.Add(AddFile("MainPage.xaml", "MauiXaml", Xaml.MainPage));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			intermediateDirectory = IOPath.Combine(tempDirectory, "obj", configuration, GetTfm());
			Build(projectFile, additionalArgs: $"-c {configuration}");

			var testDll = IOPath.Combine(intermediateDirectory, "test.dll");
			AssertExists(testDll, nonEmpty: true);
			using var assembly = AssemblyDefinition.ReadAssembly(testDll);
			var resources = assembly.MainModule.Resources.OfType<EmbeddedResource>().Select(e => e.Name).ToArray();
			if (configuration == "Debug")
			{
				// XAML files should remain as EmbeddedResource
				Assert.Contains("test.MainPage.xaml", resources);
			}
			else
			{
				// XAML files should *not* remain as EmbeddedResource
				Assert.DoesNotContain("test.MainPage.xaml", resources);
			}
		}

		[Fact(Skip = "source gen changes")]
		public void ValidateOnly_WithErrors()
		{
			SetUp();
			var project = NewProject();
			project.Add(AddFile("MainPage.xaml", "MauiXaml", Xaml.MainPage.Replace("</ContentPage>", "<NotARealThing/></ContentPage>", StringComparison.Ordinal)));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);

			string log = Build(projectFile, additionalArgs: "-p:MauiXamlCValidateOnly=True", shouldSucceed: false);
			Assert.Contains("MainPage.xaml(7,6): XamlC error XC0000: Cannot resolve type \"http://schemas.microsoft.com/dotnet/2021/maui:NotARealThing\".", log, StringComparison.Ordinal);
		}

		/// <summary>
		/// Tests that XamlG and XamlC targets skip, as well as checking IncrementalClean doesn't delete generated files
		/// </summary>
		[Fact]
		public void TargetsShouldSkip()
		{
			SetUp();
			var project = NewProject();
			project.Add(AddFile("MainPage.xaml", "MauiXaml", Xaml.MainPage));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			Build(projectFile);

			var xamlCStamp = IOPath.Combine(intermediateDirectory, "XamlC.stamp");
			AssertExists(xamlCStamp);

			var expectedXamlC = new FileInfo(xamlCStamp).LastWriteTimeUtc;

			//Build again
			Build(projectFile);
			AssertExists(xamlCStamp);

			var actualXamlC = new FileInfo(xamlCStamp).LastWriteTimeUtc;
			Assert.Equal(expectedXamlC, actualXamlC);
		}

		/// <summary>
		/// Checks that XamlG and XamlC files are cleaned
		/// </summary>
		[Fact]
		public void Clean()
		{
			SetUp();
			var project = NewProject();
			project.Add(AddFile("MainPage.xaml", "MauiXaml", Xaml.MainPage));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			Build(projectFile);

			var mainPageXamlG = IOPath.Combine(intermediateDirectory, "MainPage.xaml.g.cs");
			var fooCssG = IOPath.Combine(intermediateDirectory, "Foo.css.g.cs");
			var xamlCStamp = IOPath.Combine(intermediateDirectory, "XamlC.stamp");
			AssertExists(xamlCStamp);

			//Clean
			Build(projectFile, "Clean");
			AssertDoesNotExist(mainPageXamlG);
			AssertDoesNotExist(fooCssG);
			AssertDoesNotExist(xamlCStamp);
		}

		[Fact]
		public void LinkedFile()
		{
			SetUp();
			var folder = IOPath.Combine(tempDirectory, "A", "B");
			Directory.CreateDirectory(folder);
			File.WriteAllText(IOPath.Combine(folder, "MainPage.xaml"), Xaml.MainPage);

			var project = NewProject();
			var itemGroup = NewElement("ItemGroup");
			var embeddedResource = NewElement("MauiXaml").WithAttribute("Include", @"A\B\MainPage.xaml");
			embeddedResource.Add(NewElement("Link").WithValue(@"Pages\MainPage.xaml"));
			itemGroup.Add(embeddedResource);
			project.Add(itemGroup);
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			Build(projectFile);

			AssertExists(IOPath.Combine(intermediateDirectory, "test.dll"), nonEmpty: true);
			AssertExists(IOPath.Combine(intermediateDirectory, "XamlC.stamp"));
		}

		//https://github.com/dotnet/project-system/blob/master/docs/design-time-builds.md
		//https://daveaglick.com/posts/running-a-design-time-build-with-msbuild-apis
		[Fact]
		public void DesignTimeBuild()
		{
			SetUp();
			var project = NewProject();
			project.Add(AddFile(@"Pages\MainPage.xaml", "MauiXaml", Xaml.MainPage));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			var assembly = IOPath.Combine(intermediateDirectory, "test.dll");
			var xamlCStamp = IOPath.Combine(intermediateDirectory, "XamlC.stamp");

			if (File.Exists(xamlCStamp))
				System.IO.File.Delete(xamlCStamp);
			AssertDoesNotExist(xamlCStamp); //XamlC should be skipped

			Build(projectFile, "Compile", additionalArgs: "-p:DesignTimeBuild=True -p:BuildingInsideVisualStudio=True -p:SkipCompilerExecution=True -p:ProvideCommandLineArgs=True");


			//The assembly should not be compiled
			//AssertDoesNotExist(assembly);
			AssertDoesNotExist(xamlCStamp); //XamlC should be skipped

			//Build again, a full build
			Build(projectFile);
			AssertExists(assembly, nonEmpty: true);
			AssertExists(xamlCStamp);

		}

		[Fact]
		public void AddNewFile()
		{
			SetUp();
			var project = NewProject();
			project.Add(AddFile("MainPage.xaml", "MauiXaml", Xaml.MainPage));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			Build(projectFile);

			var xamlCStamp = IOPath.Combine(intermediateDirectory, "XamlC.stamp");
			AssertExists(xamlCStamp);

			var expectedXamlC = new FileInfo(xamlCStamp).LastWriteTimeUtc;

			//Build again, after adding a file, this triggers a full XamlG and XamlC -- *not* CssG
			project.Add(AddFile("CustomView.xaml", "MauiXaml", Xaml.CustomView));
			project.Save(projectFile);
			Build(projectFile);
			AssertExists(xamlCStamp);

			var actualXamlC = new FileInfo(xamlCStamp).LastWriteTimeUtc;
			Assert.NotEqual(expectedXamlC, actualXamlC);
		}

		[Fact(Skip = "source gen changes")]
		public void TouchXamlFile()
		{
			SetUp();
			var project = NewProject();
			project.Add(AddFile("MainPage.xaml", "MauiXaml", Xaml.MainPage));
			project.Add(AddFile("CustomView.xaml", "MauiXaml", Xaml.CustomView));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			Build(projectFile);

			var mainPageXamlG = IOPath.Combine(intermediateDirectory, "MainPage.xaml.g.cs");
			var customViewXamlG = IOPath.Combine(intermediateDirectory, "CustomView.xaml.g.cs");
			var xamlCStamp = IOPath.Combine(intermediateDirectory, "XamlC.stamp");
			AssertExists(xamlCStamp);

			var expectedCustomViewXamlG = new FileInfo(customViewXamlG).LastWriteTimeUtc;
			var expectedXamlC = new FileInfo(xamlCStamp).LastWriteTimeUtc;

			//Build again, after modifying the timestamp on a Xaml file, should trigger a partial XamlG and full XamlC
			//https://github.com/xamarin/xamarin-android/blob/61851599fb1999964bd200ec1c373b6e395933f3/src/Microsoft.Maui.Controls.Android.Build.Tasks/Utilities/MonoAndroidHelper.cs#L342
			Build(projectFile);
			AssertExists(xamlCStamp);

			var actualXamlC = new FileInfo(xamlCStamp).LastWriteTimeUtc;
			Assert.NotEqual(expectedXamlC, actualXamlC);
		}

		[Fact]
		public void RandomXml()
		{
			SetUp();
			var project = NewProject();
			project.Add(AddFile("MainPage.xaml", "MauiXaml", "<xml></xml>"));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			Build(projectFile);

			AssertExists(IOPath.Combine(intermediateDirectory, "test.dll"), nonEmpty: true);
			AssertExists(IOPath.Combine(intermediateDirectory, "XamlC.stamp"));
		}

		// [Fact]
		// public void InvalidXml()
		// {
		// 	SetUp();
		// 	var project = NewProject();
		// 	project.Add(AddFile("MainPage.xaml", "MauiXaml", "notxmlatall"));
		// 	var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
		// 	project.Save(projectFile);
		// 	Assert.Throws<XunitException>(() => Build(projectFile));
		// }

		[Fact]
		public void RandomEmbeddedResource()
		{
			SetUp();
			var project = NewProject();
			project.Add(AddFile("MainPage.xaml", "MauiXaml", Xaml.MainPage));
			project.Add(AddFile("MainPage.txt", "EmbeddedResource", "notxmlatall"));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			Build(projectFile);

			AssertExists(IOPath.Combine(intermediateDirectory, "test.dll"), nonEmpty: true);
			AssertDoesNotExist(IOPath.Combine(intermediateDirectory, "MainPage.txt.g.cs"));
			AssertExists(IOPath.Combine(intermediateDirectory, "XamlC.stamp"));
		}

		[Fact]
		public void NoXamlFiles()
		{
			SetUp();
			var project = NewProject();
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			var log = Build(projectFile, verbosity: "diagnostic");
			Assert.False(log.Contains("Building target \"XamlC\"", StringComparison.Ordinal), "XamlC should be skipped if there are no .xaml files.");
		}

		[Theory]
		[InlineData("ios", "ios;maccatalyst", true)]
		[InlineData("maccatalyst", "ios;maccatalyst", true)]
		[InlineData("android", "ios;maccatalyst", false)]
		[InlineData("ios", "ios; maccatalyst", true)]
		[InlineData("maccatalyst", "ios; maccatalyst", true)]
		[InlineData("android", "ios; maccatalyst", false)]
		public void SingleProject_SharedPlatformFolderMappingsAreRespected(string targetPlatformIdentifier, string targetPlatformIdentifiers, bool shouldIncludeAppleSharedFile)
		{
			SetUp();
			var project = NewElement("Project").WithAttribute("Sdk", "Microsoft.NET.Sdk");
			var propertyGroup = NewElement("PropertyGroup");
			propertyGroup.Add(NewElement("TargetFramework").WithValue(GetTfm()));
			propertyGroup.Add(NewElement("SingleProject").WithValue("true"));
			project.Add(propertyGroup);
			AddMauiReferences(project);
			AddSingleProjectBeforeTargetsImport(project);

			var customMappings = NewElement("ItemGroup");
			customMappings.Add(NewElement("MauiPlatformSpecificFolder")
				.WithAttribute("Include", "Platforms\\Apple\\")
				.WithAttribute("TargetPlatformIdentifiers", targetPlatformIdentifiers));
			project.Add(customMappings);

			WriteFile("Entry.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static partial class CurrentPlatform
{
}

public static class Entry
{
	public static string Value => CurrentPlatform.Name;
}");

			WriteFile("Platforms\\iOS\\CurrentPlatform.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static partial class CurrentPlatform
{
	public static string Name => ""iOS"";
}");

			WriteFile("Platforms\\MacCatalyst\\CurrentPlatform.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static partial class CurrentPlatform
{
	public static string Name => ""MacCatalyst"";
}");

			WriteFile("Platforms\\Android\\CurrentPlatform.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static partial class CurrentPlatform
{
	public static string Name => ""Android"";
}");

			WriteFile("Platforms\\Apple\\AppleSharedMarker.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class AppleSharedMarker
{
	public static string Value => ""Apple"";
}");

			AddSingleProjectTargetsImport(project);

			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);

			Build(projectFile, additionalArgs: $"-p:TargetPlatformIdentifier={targetPlatformIdentifier}");

			var testDll = IOPath.Combine(intermediateDirectory, "test.dll");
			AssertExists(testDll, nonEmpty: true);

			if (shouldIncludeAppleSharedFile)
				AssertTypeExists(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.AppleSharedMarker");
			else
				AssertTypeDoesNotExist(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.AppleSharedMarker");
		}

		[Fact]
		public void SingleProject_BuiltInPlatformFoldersCanBeExtended()
		{
			SetUp();
			var project = NewElement("Project").WithAttribute("Sdk", "Microsoft.NET.Sdk");
			var propertyGroup = NewElement("PropertyGroup");
			propertyGroup.Add(NewElement("TargetFramework").WithValue(GetTfm()));
			propertyGroup.Add(NewElement("SingleProject").WithValue("true"));
			project.Add(propertyGroup);
			AddMauiReferences(project);
			AddSingleProjectBeforeTargetsImport(project);

			var customMappings = NewElement("ItemGroup");
			var update = NewElement("MauiPlatformSpecificFolder")
				.WithAttribute("Update", "$(iOSProjectFolder)");
			update.Add(NewElement("TargetPlatformIdentifiers").WithValue("ios;maccatalyst"));
			customMappings.Add(update);
			project.Add(customMappings);

			WriteFile("Entry.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class Entry
{
	public static string Value => ExtendedIosMarker.Value;
}");

			WriteFile("Platforms\\iOS\\ExtendedIosMarker.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class ExtendedIosMarker
{
	public static string Value => ""SharedWithCatalyst"";
}");

			AddSingleProjectTargetsImport(project);

			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);

			Build(projectFile, additionalArgs: "-p:TargetPlatformIdentifier=maccatalyst");

			var testDll = IOPath.Combine(intermediateDirectory, "test.dll");
			AssertExists(testDll, nonEmpty: true);
			AssertTypeExists(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.ExtendedIosMarker");
		}

		[Theory]
		[InlineData("ios", true)]
		[InlineData("maccatalyst", false)]
		[InlineData("android", false)]
		public void SingleProject_SingularPlatformFolderMetadataRemainsBackwardCompatible(string targetPlatformIdentifier, bool shouldIncludeLegacyFile)
		{
			SetUp();
			var project = NewElement("Project").WithAttribute("Sdk", "Microsoft.NET.Sdk");
			var propertyGroup = NewElement("PropertyGroup");
			propertyGroup.Add(NewElement("TargetFramework").WithValue(GetTfm()));
			propertyGroup.Add(NewElement("SingleProject").WithValue("true"));
			project.Add(propertyGroup);
			AddMauiReferences(project);
			AddSingleProjectBeforeTargetsImport(project);

			var customMappings = NewElement("ItemGroup");
			customMappings.Add(NewElement("MauiPlatformSpecificFolder")
				.WithAttribute("Include", "Platforms\\LegacyiOS\\")
				.WithAttribute("TargetPlatformIdentifier", "ios"));
			project.Add(customMappings);

			WriteFile("Entry.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class Entry
{
	public static string Value => ""ok"";
}");

			WriteFile("Platforms\\LegacyiOS\\LegacyIosMarker.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class LegacyIosMarker
{
	public static string Value => ""LegacyiOS"";
}");

			AddSingleProjectTargetsImport(project);

			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);

			Build(projectFile, additionalArgs: $"-p:TargetPlatformIdentifier={targetPlatformIdentifier}");

			var testDll = IOPath.Combine(intermediateDirectory, "test.dll");
			AssertExists(testDll, nonEmpty: true);

			if (shouldIncludeLegacyFile)
				AssertTypeExists(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.LegacyIosMarker");
			else
				AssertTypeDoesNotExist(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.LegacyIosMarker");
		}

		[Theory]
		[InlineData(false)]
		[InlineData(true)]
		public void SingleProject_ConditionGatedPlatformFoldersCanParticipateWithoutATargetPlatformIdentifier(bool useLinuxFolder)
		{
			SetUp();
			var project = NewElement("Project").WithAttribute("Sdk", "Microsoft.NET.Sdk");
			var propertyGroup = NewElement("PropertyGroup");
			propertyGroup.Add(NewElement("TargetFramework").WithValue(GetTfm()));
			propertyGroup.Add(NewElement("SingleProject").WithValue("true"));
			project.Add(propertyGroup);
			AddMauiReferences(project);
			AddSingleProjectBeforeTargetsImport(project);

			var customMappings = NewElement("ItemGroup");
			customMappings.Add(NewElement("MauiPlatformSpecificFolder")
				.WithAttribute("Include", "Platforms\\Linux\\")
				.WithAttribute("Condition", " '$(UseLinuxFolder)' == 'true' "));
			project.Add(customMappings);

			WriteFile("Entry.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class Entry
{
	public static string Value => ""ok"";
}");

			WriteFile("Platforms\\Linux\\LinuxMarker.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class LinuxMarker
{
	public static string Value => ""Linux"";
}");

			AddSingleProjectTargetsImport(project);

			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);

			Build(projectFile, additionalArgs: $"-p:UseLinuxFolder={useLinuxFolder.ToString().ToLowerInvariant()}");

			var testDll = IOPath.Combine(intermediateDirectory, "test.dll");
			AssertExists(testDll, nonEmpty: true);

			if (useLinuxFolder)
				AssertTypeExists(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.LinuxMarker");
			else
				AssertTypeDoesNotExist(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.LinuxMarker");
		}

		/// <summary>
		/// Tests that the SingleProject Before targets respect custom CodesignEntitlements properties
		/// </summary>
		[Fact]
		public void SingleProject_CodesignEntitlementsRespected()
		{
			SetUp();
			// Create a minimal project for property evaluation testing only
			var project = NewElement("Project").WithAttribute("Sdk", "Microsoft.NET.Sdk");

			// Add PropertyGroup with test properties
			var propertyGroup = NewElement("PropertyGroup");
			propertyGroup.Add(NewElement("TargetFramework").WithValue(GetTfm()));
			propertyGroup.Add(NewElement("SingleProject").WithValue("true"));

			// Test scenario 1: Custom CodesignEntitlements should be preserved
			propertyGroup.Add(NewElement("CodesignEntitlements").WithValue("Custom\\Entitlements.plist"));
			propertyGroup.Add(NewElement("iOSProjectFolder").WithValue("Platforms\\iOS\\"));
			propertyGroup.Add(NewElement("MacCatalystProjectFolder").WithValue("Platforms\\MacCatalyst\\"));
			project.Add(propertyGroup);

			// Add import for the SingleProject Before targets we're testing
			var targetsPath = AssemblyInfoTests.GetFilePathFromRoot(IOPath.Combine("src", "Controls", "src", "Build.Tasks", "nuget", "buildTransitive", "netstandard2.0", "Microsoft.Maui.Controls.SingleProject.Before.targets"));
			var import = NewElement("Import")
				.WithAttribute("Project", targetsPath);
			project.Add(import);

			// Create the entitlements files
			project.Add(AddFile("Platforms\\iOS\\Entitlements.plist", "None", "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<plist version=\"1.0\"><dict></dict></plist>"));
			project.Add(AddFile("Custom\\Entitlements.plist", "None", "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<plist version=\"1.0\"><dict></dict></plist>"));

			// Add a target to output the CodesignEntitlements property value for verification
			var target = NewElement("Target").WithAttribute("Name", "TestCodesignEntitlements");
			target.Add(NewElement("Message")
				.WithAttribute("Text", "CodesignEntitlements = $(CodesignEntitlements)")
				.WithAttribute("Importance", "high"));
			project.Add(target);

			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);

			// Build the test target to see property evaluation
			var log = Build(projectFile, target: "TestCodesignEntitlements", verbosity: "normal");

			// Verify the custom CodesignEntitlements property is preserved
			Assert.True(log.Contains("CodesignEntitlements = Custom/Entitlements.plist", StringComparison.Ordinal) ||
						  log.Contains("CodesignEntitlements = Custom\\Entitlements.plist", StringComparison.Ordinal),
				"Custom CodesignEntitlements property should be preserved and not overridden by default Entitlements.plist");
		}

		/// <summary>
		/// Tests that the SingleProject Before targets use default Entitlements.plist when no custom CodesignEntitlements is set
		/// </summary>
		[Fact]
		public void SingleProject_DefaultEntitlementsUsedWhenNoCustomSet()
		{
			SetUp();
			// Create a minimal project for property evaluation testing only
			var project = NewElement("Project").WithAttribute("Sdk", "Microsoft.NET.Sdk");

			// Add PropertyGroup with test properties - NO CodesignEntitlements set
			var propertyGroup = NewElement("PropertyGroup");
			propertyGroup.Add(NewElement("TargetFramework").WithValue(GetTfm()));
			propertyGroup.Add(NewElement("SingleProject").WithValue("true"));
			propertyGroup.Add(NewElement("iOSProjectFolder").WithValue("Platforms\\iOS\\"));
			propertyGroup.Add(NewElement("MacCatalystProjectFolder").WithValue("Platforms\\MacCatalyst\\"));
			// Simulate the iOS platform for testing purposes
			propertyGroup.Add(NewElement("_TestiOSCondition").WithValue("true"));
			project.Add(propertyGroup);

			// Add import for the SingleProject Before targets we're testing
			var targetsPath = AssemblyInfoTests.GetFilePathFromRoot(IOPath.Combine("src", "Controls", "src", "Build.Tasks", "nuget", "buildTransitive", "netstandard2.0", "Microsoft.Maui.Controls.SingleProject.Before.targets"));
			var import = NewElement("Import")
				.WithAttribute("Project", targetsPath);
			project.Add(import);

			// Create the default entitlements file
			project.Add(AddFile("Platforms\\iOS\\Entitlements.plist", "None", "<?xml version=\"1.0\" encoding=\"UTF-8\"?>\n<plist version=\"1.0\"><dict></dict></plist>"));

			// Add a PropertyGroup that simulates the iOS condition for testing
			var testConditionGroup = NewElement("PropertyGroup").WithAttribute("Condition", " '$(SingleProject)' == 'true' and '$(TFMTestiOSCondition)' == 'true' ");
			testConditionGroup.Add(NewElement("CodesignEntitlements")
				.WithAttribute("Condition", " '$(CodesignEntitlements)' == '' and Exists('$(iOSProjectFolder)Entitlements.plist') ")
				.WithValue("$(iOSProjectFolder)Entitlements.plist"));
			project.Add(testConditionGroup);

			// Add a target to output the CodesignEntitlements property value for verification
			var target = NewElement("Target").WithAttribute("Name", "TestCodesignEntitlements");
			target.Add(NewElement("Message")
				.WithAttribute("Text", "CodesignEntitlements = $(CodesignEntitlements)")
				.WithAttribute("Importance", "high"));
			project.Add(target);

			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);

			// Build the test target to see property evaluation - enable the test condition
			var log = Build(projectFile, target: "TestCodesignEntitlements", verbosity: "normal", additionalArgs: "-p:TFMTestiOSCondition=true");

			// Verify the default CodesignEntitlements property is used
			Assert.True(log.Contains("CodesignEntitlements = Platforms/iOS/Entitlements.plist", StringComparison.Ordinal) ||
						  log.Contains("CodesignEntitlements = Platforms\\iOS\\Entitlements.plist", StringComparison.Ordinal),
				"Default Entitlements.plist should be used when no custom CodesignEntitlements is set");
		}
	}
}
