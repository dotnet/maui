using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Xml.Linq;
using Mono.Cecil;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using static Microsoft.Maui.Controls.MSBuild.UnitTests.MSBuildXmlExtensions;
using IOPath = System.IO.Path;

namespace Microsoft.Maui.Controls.MSBuild.UnitTests
{
	//This set of tests is for validating Microsoft.Maui.Controls.targets
	[TestFixture]
	[Category("LongRunning")]
	public class MSBuildTests
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

		string testDirectory;
		string tempDirectory;
		string intermediateDirectory;

		[SetUp]
		public void SetUp()
		{
			testDirectory = TestContext.CurrentContext.TestDirectory;
			tempDirectory = IOPath.Combine(testDirectory, "temp",
				TestContext.CurrentContext.Test.Name
					.Replace('"', '_')
					.Replace('(', '_')
					.Replace(')', '_'));
			intermediateDirectory = IOPath.Combine(tempDirectory, "obj", "Debug", GetTfm());
			Directory.CreateDirectory(tempDirectory);

			//copy _Directory.Build.[props|targets] in test/
			var props = AssemblyInfoTests.GetFilePathFromRoot(IOPath.Combine("src", "Controls", "tests", "Xaml.UnitTests", "MSBuild", "_Directory.Build.props"));
			var targets = AssemblyInfoTests.GetFilePathFromRoot(IOPath.Combine("src", "Controls", "tests", "Xaml.UnitTests", "MSBuild", "_Directory.Build.targets"));

			if (!File.Exists(props))
			{
				//NOTE: VSTS may be running tests in a staging directory, so we can use an environment variable to find the source
				//https://docs.microsoft.com/en-us/vsts/build-release/concepts/definitions/build/variables?view=vsts&tabs=batch#buildsourcesdirectory
				Assert.Fail("Unable to find _Directory.Build.props at path: " + props);
			}

			File.Copy(props, IOPath.Combine(tempDirectory, "Directory.Build.props"), true);
			File.Copy(targets, IOPath.Combine(tempDirectory, "Directory.Build.targets"), true);
		}

		[TearDown]
		public void TearDown()
		{
			// Leave log files behind on test failures
			if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Failed)
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

		string Build(string projectFile, string target = "Build", string verbosity = "normal", string additionalArgs = "", bool shouldSucceed = true)
		{
			var builder = new StringBuilder();
			void onData(object s, DataReceivedEventArgs e)
			{
				lock (builder)
					if (e.Data != null)
					{
						builder.AppendLine(e.Data);
						Console.WriteLine(e.Data);
					}
			}
			;

			var ext = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ".exe" : "";
			var dotnet = IOPath.Combine(testDirectory, "..", "..", "..", "..", "..", "..", "..", "bin", "dotnet", $"dotnet{ext}");
			if (!File.Exists(dotnet))
			{
				Console.WriteLine($"Using 'dotnet', did not find: {dotnet}");

				// If we don't have .\bin\dotnet\dotnet, try the system one
				dotnet = "dotnet";
			}
			else
			{
				dotnet = IOPath.GetFullPath(dotnet);

				Console.WriteLine($"Using '{dotnet}'");
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

				// Add the binlog as a test attachment
				var binlog = IOPath.Combine(IOPath.GetDirectoryName(projectFile), "msbuild.binlog");
				if (File.Exists(binlog))
				{
					TestContext.AddTestAttachment(binlog);
				}

				if (shouldSucceed)
					Assert.AreEqual(0, p.ExitCode, "MSBuild exited with {0}", p.ExitCode);
				else
					Assert.AreNotEqual(0, p.ExitCode, "MSBuild exited with {0}", p.ExitCode);

				return builder.ToString();
			}
		}

		void AssertExists(string path, bool nonEmpty = false)
		{
			if (!File.Exists(path))
				Assert.Fail($"{path} should exist!");

			if (nonEmpty && new FileInfo(path).Length == 0)
				Assert.Fail($"{path} is empty!");
		}

		void AssertDoesNotExist(string path)
		{
			if (File.Exists(path))
				Assert.Fail($"{path} should *not* exist!");
		}

		[Test]
		public void BuildAProject()
		{
			var project = NewProject();
			project.Add(AddFile("MainPage.xaml", "MauiXaml", Xaml.MainPage));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			Build(projectFile);

			AssertExists(IOPath.Combine(intermediateDirectory, "test.dll"), nonEmpty: true);
			AssertExists(IOPath.Combine(intermediateDirectory, "XamlC.stamp"));
		}

		// Tests the MauiXamlCValidateOnly=True MSBuild property
		[Test]
		public void ValidateOnly([Values("Debug", "Release", "ReleaseProd")] string configuration)
		{
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
				CollectionAssert.Contains(resources, "test.MainPage.xaml");
			}
			else
			{
				// XAML files should *not* remain as EmbeddedResource
				CollectionAssert.DoesNotContain(resources, "test.MainPage.xaml");
			}
		}

		[Test]
		[Ignore("source gen changes")]
		public void ValidateOnly_WithErrors()
		{
			var project = NewProject();
			project.Add(AddFile("MainPage.xaml", "MauiXaml", Xaml.MainPage.Replace("</ContentPage>", "<NotARealThing/></ContentPage>", StringComparison.Ordinal)));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);

			string log = Build(projectFile, additionalArgs: "-p:MauiXamlCValidateOnly=True", shouldSucceed: false);
			StringAssert.Contains("MainPage.xaml(7,6): XamlC error XC0000: Cannot resolve type \"http://schemas.microsoft.com/dotnet/2021/maui:NotARealThing\".", log);
		}

		/// <summary>
		/// Tests that XamlG and XamlC targets skip, as well as checking IncrementalClean doesn't delete generated files
		/// </summary>
		[Test]
		public void TargetsShouldSkip()
		{
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
			Assert.AreEqual(expectedXamlC, actualXamlC, $"Timestamps should match for {xamlCStamp}.");
		}

		/// <summary>
		/// Checks that XamlG and XamlC files are cleaned
		/// </summary>
		[Test]
		public void Clean()
		{
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

		[Test]
		public void LinkedFile()
		{
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
		[Test]
		public void DesignTimeBuild()
		{
			var project = NewProject();
			project.Add(AddFile(@"Pages\MainPage.xaml", "MauiXaml", Xaml.MainPage));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);

			Build(projectFile, "Compile", additionalArgs: "-p:DesignTimeBuild=True -p:BuildingInsideVisualStudio=True -p:SkipCompilerExecution=True -p:ProvideCommandLineArgs=True");

			var assembly = IOPath.Combine(intermediateDirectory, "test.dll");
			var xamlCStamp = IOPath.Combine(intermediateDirectory, "XamlC.stamp");

			//The assembly should not be compiled
			AssertDoesNotExist(assembly);
			AssertDoesNotExist(xamlCStamp); //XamlC should be skipped

			//Build again, a full build
			Build(projectFile);
			AssertExists(assembly, nonEmpty: true);
			AssertExists(xamlCStamp);

		}

		[Test]
		public void AddNewFile()
		{
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
			Assert.AreNotEqual(expectedXamlC, actualXamlC, $"Timestamps should *not* match for {xamlCStamp}.");
		}

		[Test]
		[Ignore("source gen changes")]
		public void TouchXamlFile()
		{
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
			Assert.AreNotEqual(expectedXamlC, actualXamlC, $"Timestamps should *not* match for {xamlCStamp}.");
		}

		[Test]
		public void RandomXml()
		{
			var project = NewProject();
			project.Add(AddFile("MainPage.xaml", "MauiXaml", "<xml></xml>"));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			Build(projectFile);

			AssertExists(IOPath.Combine(intermediateDirectory, "test.dll"), nonEmpty: true);
			AssertExists(IOPath.Combine(intermediateDirectory, "XamlC.stamp"));
		}

		// [Test]
		// public void InvalidXml()
		// {
		// 	var project = NewProject();
		// 	project.Add(AddFile("MainPage.xaml", "MauiXaml", "notxmlatall"));
		// 	var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
		// 	project.Save(projectFile);
		// 	Assert.Throws<AssertionException>(() => Build(projectFile));
		// }

		[Test]
		public void RandomEmbeddedResource()
		{
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

		[Test]
		public void NoXamlFiles()
		{
			var project = NewProject();
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			var log = Build(projectFile, verbosity: "diagnostic");
			Assert.IsTrue(log.Contains("Target \"XamlC\" skipped", StringComparison.Ordinal), "XamlC should be skipped if there are no .xaml files.");
		}
	}
}
