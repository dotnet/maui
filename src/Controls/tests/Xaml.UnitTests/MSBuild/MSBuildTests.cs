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
	// This set of tests is for validating Microsoft.Maui.Controls.targets.
	//
	// Contract under test: SourceGen is the default XAML inflator, and the legacy
	// `XamlC` MSBuild target must NOT run on a default build. Tests verify:
	//   1. XamlC is SKIPPED when all XAML uses SourceGen (BuildAProject, LinkedFile,
	//      RandomXml, RandomEmbeddedResource, NoXamlFiles).
	//   2. XamlC DOES run when files use the XamlC or Runtime inflator
	//      (XamlCRunsWhenDeprecatedInflatorUsed, covering both gate branches).
	[Trait("Category", "LongRunning")]
	public class MSBuildTests : IDisposable
	{
		static readonly string[] references = {
			"Microsoft.Maui.Controls.dll",
			"Microsoft.Maui.Controls.Xaml.dll",
			"Microsoft.Maui.dll",
		};

		/// <summary>
		/// Opts a test into the XamlC inflator — for tests whose subject is the XamlC build target,
		/// which is gated off under the SourceGen default. Bare "XamlC" (not "SourceGen,XamlC")
		/// avoids two traps: commas in -p: values split as CLI switches, and the combo produces a
		/// duplicate InitializeComponent (CS0111). WarningsNotAsErrors=MAUI1001 silences the
		/// inflator-deprecation warning.
		/// </summary>
		private const string XamlCOptIn = "-p:MauiXamlInflator=XamlC -p:WarningsNotAsErrors=MAUI1001";

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

			// Legacy assembly-wide XamlC attribute. The inflator is now chosen by MauiXamlInflator,
			// so this is a no-op — the skip tests rely on it NOT forcing the XamlC target.
			project.Add(AddFile("AssemblyInfo.cs", "Compile", "#pragma warning disable CS0618\n[assembly: Microsoft.Maui.Controls.Xaml.XamlCompilation (Microsoft.Maui.Controls.Xaml.XamlCompilationOptions.Compile)]"));

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

		void WriteFile(string name, string contents)
		{
			var filePath = IOPath.Combine(tempDirectory, name.Replace('\\', IOPath.DirectorySeparatorChar).Replace('/', IOPath.DirectorySeparatorChar));
			Directory.CreateDirectory(IOPath.GetDirectoryName(filePath));
			File.WriteAllText(filePath, contents);
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

			// Assign the synthetic TargetPlatformIdentifier from a private test-only property
			// inside a target rather than as a global 'dotnet build' property. Passing a platform
			// TPI globally makes the SDK attempt workload resolution during evaluation for what is a
			// plain net11.0 (non-platform) project, which fails on CI agents without that workload
			// (NETSDK1208 / NETSDK1178) before the SingleProject targets under test ever run. Setting
			// it here — after SDK evaluation but before the SingleProject compile-filtering targets —
			// keeps the tests workload-neutral while still exercising the TPI-dependent logic (the
			// allow-list built by _MauiCollectPlatformSpecificCompileItems determines which files are
			// compiled, so it does not rely on the evaluation-time per-TPI Compile metadata flip).
			var applyTpiTarget = NewElement("Target")
				.WithAttribute("Name", "_ApplyTestTargetPlatformIdentifier")
				.WithAttribute("BeforeTargets", "_MauiNormalizePlatformSpecificFolders;_MauiCollectPlatformSpecificCompileItems;_MauiRemovePlatformCompileItems")
				.WithAttribute("Condition", " '$(_SingleProjectTestTargetPlatformIdentifier)' != '' ");
			var tpiPropertyGroup = NewElement("PropertyGroup");
			tpiPropertyGroup.Add(NewElement("TargetPlatformIdentifier").WithValue("$(_SingleProjectTestTargetPlatformIdentifier)"));
			applyTpiTarget.Add(tpiPropertyGroup);
			project.Add(applyTpiTarget);
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
			// Default inflator is SourceGen, so the XamlC target is skipped and no stamp is produced.
			AssertDoesNotExist(IOPath.Combine(intermediateDirectory, "XamlC.stamp"));
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

		// Tests the default build behavior with SourceGen inflator
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
			// With SourceGen as default inflator, XAML files are not embedded as resources
			Assert.DoesNotContain("test.MainPage.xaml", resources);
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
			// XamlC subject test — see XamlCOptIn doc
			Build(projectFile, additionalArgs: XamlCOptIn);

			var xamlCStamp = IOPath.Combine(intermediateDirectory, "XamlC.stamp");
			AssertExists(xamlCStamp);

			var expectedXamlC = new FileInfo(xamlCStamp).LastWriteTimeUtc;

			//Build again
			Build(projectFile, additionalArgs: XamlCOptIn);
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
			// XamlC subject test — see XamlCOptIn doc.
			// Clean keys off <FileWrites> recorded during this build, so the Clean invocation below does not need it.
			Build(projectFile, additionalArgs: XamlCOptIn);

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
			// Default inflator is SourceGen, so the XamlC target is skipped and no stamp is produced.
			AssertDoesNotExist(IOPath.Combine(intermediateDirectory, "XamlC.stamp"));
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
			AssertDoesNotExist(xamlCStamp); // precondition: stamp cleared before DTB build

			Build(projectFile, "Compile", additionalArgs: "-p:DesignTimeBuild=True -p:BuildingInsideVisualStudio=True -p:SkipCompilerExecution=True -p:ProvideCommandLineArgs=True " + XamlCOptIn);

			//The assembly should not be compiled
			//AssertDoesNotExist(assembly);
			AssertDoesNotExist(xamlCStamp); //XamlC should be skipped

			//Build again, a full build
			// XamlC subject test — see XamlCOptIn doc
			Build(projectFile, additionalArgs: XamlCOptIn);
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
			// XamlC subject test — see XamlCOptIn doc
			Build(projectFile, additionalArgs: XamlCOptIn);

			var xamlCStamp = IOPath.Combine(intermediateDirectory, "XamlC.stamp");
			AssertExists(xamlCStamp);

			var expectedXamlC = new FileInfo(xamlCStamp).LastWriteTimeUtc;

			//Build again, after adding a file, this triggers a full XamlG and XamlC -- *not* CssG
			project.Add(AddFile("CustomView.xaml", "MauiXaml", Xaml.CustomView));
			project.Save(projectFile);
			Build(projectFile, additionalArgs: XamlCOptIn);
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
			// XamlC subject test — see XamlCOptIn doc. (Test currently skipped; opt-in kept for future-proofing.)
			Build(projectFile, additionalArgs: XamlCOptIn);

			var mainPageXamlG = IOPath.Combine(intermediateDirectory, "MainPage.xaml.g.cs");
			var customViewXamlG = IOPath.Combine(intermediateDirectory, "CustomView.xaml.g.cs");
			var xamlCStamp = IOPath.Combine(intermediateDirectory, "XamlC.stamp");
			AssertExists(xamlCStamp);

			var expectedCustomViewXamlG = new FileInfo(customViewXamlG).LastWriteTimeUtc;
			var expectedXamlC = new FileInfo(xamlCStamp).LastWriteTimeUtc;

			//Build again, after modifying the timestamp on a Xaml file, should trigger a partial XamlG and full XamlC
			//https://github.com/xamarin/xamarin-android/blob/61851599fb1999964bd200ec1c373b6e395933f3/src/Microsoft.Maui.Controls.Android.Build.Tasks/Utilities/MonoAndroidHelper.cs#L342
			Build(projectFile, additionalArgs: XamlCOptIn);
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
			// Default inflator is SourceGen, so the XamlC target is skipped and no stamp is produced.
			AssertDoesNotExist(IOPath.Combine(intermediateDirectory, "XamlC.stamp"));
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
			// Default inflator is SourceGen, so the XamlC target is skipped and no stamp is produced.
			AssertDoesNotExist(IOPath.Combine(intermediateDirectory, "XamlC.stamp"));
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

		/// <summary>
		/// Positive counterpart to BuildAProject: XamlC runs when a deprecated inflator is selected.
		/// Covers both gate branches — _MauiXaml_XC (XamlC) and _MauiXaml_RT (Runtime) — so neither
		/// can be dropped without failing a test.
		/// </summary>
		[Theory]
		[InlineData("XamlC")]
		[InlineData("Runtime")]
		public void XamlCRunsWhenDeprecatedInflatorUsed(string inflator)
		{
			// Per-inflator directory so the two Theory rows can't leak a stale XamlC.stamp between them.
			SetUp($"{nameof(XamlCRunsWhenDeprecatedInflatorUsed)}_{inflator}");
			var project = NewProject();
			project.Add(AddFile("MainPage.xaml", "MauiXaml", Xaml.MainPage));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			// WarningsNotAsErrors silences the MAUI1001 inflator-deprecation warning. The older
			// "-warnaserror-:" syntax is rejected by newer MSBuild, so use the property form.
			Build(projectFile, additionalArgs: $"-p:MauiXamlInflator={inflator} -p:WarningsNotAsErrors=MAUI1001");

			AssertExists(IOPath.Combine(intermediateDirectory, "test.dll"), nonEmpty: true);
			// XamlC or Runtime populates _MauiXaml_XC/_RT, so the XamlC target runs.
			AssertExists(IOPath.Combine(intermediateDirectory, "XamlC.stamp"));
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

		// --- SingleProject platform-folder registration & activation ---------------
		//
		// These tests exercise the data-driven MauiPlatformSpecificFolder compile
		// selection contract: recognized TargetPlatformIdentifier(s) matching, shared
		// folders, backward-compatible singular metadata, condition-gated folders, and
		// the neutral-TFM backend activation added for external backends (Part of
		// #35021 / Part of #36650). They build a minimal SingleProject csproj that
		// imports the real SingleProject Before/After targets so the actual shipping
		// logic is under test rather than a copy.

		[Theory]
		[InlineData("ios", "ios;maccatalyst", true)]
		[InlineData("maccatalyst", "ios;maccatalyst", true)]
		[InlineData("android", "ios;maccatalyst", false)]
		[InlineData("ios", "ios; maccatalyst", true)]
		[InlineData("maccatalyst", "ios; maccatalyst", true)]
		[InlineData("android", "ios; maccatalyst", false)]
		// Tab and mixed whitespace in the list — see Regex.Replace(\s+, '') in
		// _MauiCollectPlatformSpecificCompileItems. ASCII-space-only stripping
		// (.Replace(' ', '')) would silently miss these and break shared folders.
		[InlineData("ios", "ios;\tmaccatalyst", true)]
		[InlineData("maccatalyst", "ios;\tmaccatalyst", true)]
		[InlineData("ios", "ios; \t maccatalyst", true)]
		[InlineData("maccatalyst", "ios; \t maccatalyst", true)]
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

public static class Entry
{
	public static string Value => ""ok"";
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

			Build(projectFile, additionalArgs: $"-p:_SingleProjectTestTargetPlatformIdentifier={targetPlatformIdentifier}");

			var testDll = IOPath.Combine(intermediateDirectory, "test.dll");
			AssertExists(testDll, nonEmpty: true);

			if (shouldIncludeAppleSharedFile)
				AssertTypeExists(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.AppleSharedMarker");
			else
				AssertTypeDoesNotExist(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.AppleSharedMarker");
		}

		// Regression test: a user-supplied folder declared without a trailing slash
		// must NOT match sibling folders sharing a common prefix. Without
		// EnsureTrailingSlash() in _MauiCollectPlatformSpecificCompileItems, the
		// glob "Platforms\Apple**/*.cs" would silently include AppleX/AppleLegacy.
		[Theory]
		[InlineData("Platforms\\Apple", "ios")]
		[InlineData("Platforms\\Apple\\", "ios")]
		[InlineData("Platforms\\Apple", "maccatalyst")]
		[InlineData("Platforms\\Apple\\", "maccatalyst")]
		public void SingleProject_PlatformFolderWithoutTrailingSlashDoesNotMatchSiblingFolders(string includePath, string targetPlatformIdentifier)
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
				.WithAttribute("Include", includePath)
				.WithAttribute("TargetPlatformIdentifiers", "ios;maccatalyst"));
			project.Add(customMappings);

			WriteFile("Entry.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class Entry
{
	public static string Value => ""ok"";
}");

			WriteFile("Platforms\\Apple\\AppleSharedMarker.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class AppleSharedMarker
{
	public static string Value => ""Apple"";
}");

			// Sibling folder with a common prefix — must NOT be picked up by the
			// "Apple" mapping regardless of trailing-slash authoring.
			WriteFile("Platforms\\AppleX\\AppleXMarker.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class AppleXMarker
{
	public static string Value => ""AppleX"";
}");

			AddSingleProjectTargetsImport(project);

			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);

			Build(projectFile, additionalArgs: $"-p:_SingleProjectTestTargetPlatformIdentifier={targetPlatformIdentifier}");

			var testDll = IOPath.Combine(intermediateDirectory, "test.dll");
			AssertExists(testDll, nonEmpty: true);
			AssertTypeExists(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.AppleSharedMarker");
			AssertTypeDoesNotExist(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.AppleXMarker");
		}

		// Non-platform builds (TargetPlatformIdentifier empty, e.g. design-time
		// or netstandard TFM) must keep removing platform folders that declare a
		// non-empty TargetPlatformIdentifiers. An unconditioned shared folder with
		// empty TargetPlatformIdentifiers and empty ActivationValue must still
		// participate — locks in the "empty TPI + empty ActivationValue = always
		// include" branch of _MauiCollectPlatformSpecificCompileItems. (Genuine
		// item-Condition gating — where the mapping carries its own Condition — is
		// covered separately by
		// SingleProject_ConditionGatedFolderParticipatesOnlyWhenConditionIsTrue.)
		[Fact]
		public void SingleProject_NonPlatformBuildExcludesPlatformSpecificFoldersButKeepsSharedFolder()
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
				.WithAttribute("TargetPlatformIdentifiers", "ios;maccatalyst"));
			customMappings.Add(NewElement("MauiPlatformSpecificFolder")
				.WithAttribute("Include", "Platforms\\Shared\\"));
			project.Add(customMappings);

			WriteFile("Entry.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class Entry
{
	public static string Value => ""ok"";
}");

			WriteFile("Platforms\\Apple\\AppleSharedMarker.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class AppleSharedMarker
{
	public static string Value => ""Apple"";
}");

			WriteFile("Platforms\\Shared\\SharedMarker.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class SharedMarker
{
	public static string Value => ""Shared"";
}");

			AddSingleProjectTargetsImport(project);

			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);

			// No -p:_SingleProjectTestTargetPlatformIdentifier — simulates the
			// non-platform TFM / design-time evaluation scenario.
			Build(projectFile);

			var testDll = IOPath.Combine(intermediateDirectory, "test.dll");
			AssertExists(testDll, nonEmpty: true);
			AssertTypeDoesNotExist(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.AppleSharedMarker");
			AssertTypeExists(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.SharedMarker");
		}

		// Genuine item-Condition gating: a MauiPlatformSpecificFolder mapping may
		// carry its own MSBuild item Condition that decides participation at
		// evaluation time. When the Condition is true the item materializes and —
		// declaring neither TargetPlatformIdentifiers nor an ActivationValue — is
		// kept via the "always include" branch of
		// _MauiCollectPlatformSpecificCompileItems, so its Platforms/<Folder>/**/*.cs
		// compile even on a non-platform build. When the Condition is false the item
		// never exists and _MauiRemovePlatformCompileItems strips the folder like any
		// other Platforms/** content. Exercises both branches through the actual
		// shipping SingleProject targets.
		[Theory]
		[InlineData("true", true)]
		[InlineData("false", false)]
		public void SingleProject_ConditionGatedFolderParticipatesOnlyWhenConditionIsTrue(string conditionValue, bool shouldIncludeConditionalFile)
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
			// An unconditioned shared folder guarantees @(MauiPlatformSpecificFolder)
			// is non-empty in both branches, so the false case still flows through the
			// collect/remove pipeline rather than short-circuiting on an empty list.
			customMappings.Add(NewElement("MauiPlatformSpecificFolder")
				.WithAttribute("Include", "Platforms\\Shared\\"));
			customMappings.Add(NewElement("MauiPlatformSpecificFolder")
				.WithAttribute("Include", "Platforms\\Conditional\\")
				.WithAttribute("Condition", " '$(IncludeConditionalBackend)' == 'true' "));
			project.Add(customMappings);

			WriteFile("Entry.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class Entry
{
	public static string Value => ""ok"";
}");

			WriteFile("Platforms\\Shared\\SharedMarker.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class SharedMarker
{
	public static string Value => ""Shared"";
}");

			WriteFile("Platforms\\Conditional\\ConditionalMarker.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class ConditionalMarker
{
	public static string Value => ""Conditional"";
}");

			AddSingleProjectTargetsImport(project);

			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);

			// No _SingleProjectTestTargetPlatformIdentifier — non-platform build; the
			// property toggles the mapping's item Condition to true/false.
			Build(projectFile, additionalArgs: $"-p:IncludeConditionalBackend={conditionValue}");

			var testDll = IOPath.Combine(intermediateDirectory, "test.dll");
			AssertExists(testDll, nonEmpty: true);
			// The unconditioned shared folder is always kept regardless of the gate.
			AssertTypeExists(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.SharedMarker");

			if (shouldIncludeConditionalFile)
				AssertTypeExists(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.ConditionalMarker");
			else
				AssertTypeDoesNotExist(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.ConditionalMarker");
		}

		// Design-time metadata contract for _MauiUnflipKeptCompileItemMetadata.
		// The blanket <Compile Update> marks every Platforms/** file
		// ExcludeFromCurrentConfiguration=true, and the per-TPI flips only cover the
		// built-in $(<TPI>ProjectFolder) paths. A kept shared folder (empty TPI +
		// empty ActivationValue, so it survives the blanket removal) therefore relies
		// on _MauiUnflipKeptCompileItemMetadata to flip its Compile item back to
		// ExcludeFromCurrentConfiguration=false so Visual Studio does not grey it out.
		// This asserts the actual metadata value on the kept item via a diagnostic
		// Message target that runs after the shipping unflip target on a non-platform
		// (design-time-style) build.
		[Fact]
		public void SingleProject_UnflipKeptCompileItemMetadata_SetsExcludeFromCurrentConfigurationFalseForKeptSharedFolder()
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
				.WithAttribute("Include", "Platforms\\Shared\\"));
			project.Add(customMappings);

			WriteFile("Entry.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class Entry
{
	public static string Value => ""ok"";
}");

			WriteFile("Platforms\\Shared\\SharedMarker.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class SharedMarker
{
	public static string Value => ""Shared"";
}");

			AddSingleProjectTargetsImport(project);

			// Diagnostic target: after the shipping unflip target runs, emit the
			// ExcludeFromCurrentConfiguration metadata for each surviving Compile item
			// (one Message per item via cross-item batching) so the test can assert the
			// kept shared file was flipped back to false.
			var dumpTarget = NewElement("Target")
				.WithAttribute("Name", "_TestDumpExcludeFromCurrentConfiguration")
				.WithAttribute("AfterTargets", "_MauiUnflipKeptCompileItemMetadata");
			dumpTarget.Add(NewElement("Message")
				.WithAttribute("Importance", "high")
				.WithAttribute("Condition", " '%(Compile.Identity)' != '' ")
				.WithAttribute("Text", "COMPILE_META: %(Compile.Identity)|ExcludeFromCurrentConfiguration=%(Compile.ExcludeFromCurrentConfiguration)"));
			project.Add(dumpTarget);

			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);

			// Non-platform build (no _SingleProjectTestTargetPlatformIdentifier) mirrors
			// the design-time evaluation where the IDE would otherwise grey out the file.
			var log = Build(projectFile);

			var testDll = IOPath.Combine(intermediateDirectory, "test.dll");
			AssertExists(testDll, nonEmpty: true);
			AssertTypeExists(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.SharedMarker");

			// The kept shared file must carry ExcludeFromCurrentConfiguration=false; the
			// pipe format keeps the identity (which may use either path separator) and
			// the metadata on the same emitted line.
			Assert.True(
				log.Contains("SharedMarker.cs|ExcludeFromCurrentConfiguration=false", StringComparison.OrdinalIgnoreCase),
				"_MauiUnflipKeptCompileItemMetadata should flip ExcludeFromCurrentConfiguration back to false " +
				"on the kept shared folder's Compile item. Build log:\n" + log);
		}

		// Backward compatibility: a folder that declares only the legacy singular
		// TargetPlatformIdentifier metadata must continue to match exactly that TPI.
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

			Build(projectFile, additionalArgs: $"-p:_SingleProjectTestTargetPlatformIdentifier={targetPlatformIdentifier}");

			var testDll = IOPath.Combine(intermediateDirectory, "test.dll");
			AssertExists(testDll, nonEmpty: true);

			if (shouldIncludeLegacyFile)
				AssertTypeExists(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.LegacyIosMarker");
			else
				AssertTypeDoesNotExist(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.LegacyIosMarker");
		}

		// Neutral-TFM activation (the GTK scenario from #35021/#36650). On a plain
		// net11.0 inner build no TargetPlatformIdentifier is recognized; a backend
		// declares a stable BackendIdentity and is activated via the well-known
		// MauiActiveBackend selector. Only the activated backend's Platforms/<Backend>
		// files compile; a recognized built-in folder (iOS) stays excluded because
		// its TPI does not match the (empty) neutral TPI.
		[Fact]
		public void SingleProject_NeutralTfmActivatesBackendByIdentity()
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
				.WithAttribute("Include", "Platforms\\Gtk\\")
				.WithAttribute("BackendIdentity", "gtk"));
			project.Add(customMappings);

			WriteFile("Entry.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class Entry
{
	public static string Value => ""ok"";
}");

			WriteFile("Platforms\\Gtk\\GtkMarker.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class GtkMarker
{
	public static string Value => ""Gtk"";
}");

			// A built-in recognized folder (iOS) that must NOT come in on a neutral
			// build — proves activation is exclusive to the selected backend.
			WriteFile("Platforms\\iOS\\IosMarker.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class IosMarker
{
	public static string Value => ""iOS"";
}");

			AddSingleProjectTargetsImport(project);

			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);

			// Neutral TFM (no _SingleProjectTestTargetPlatformIdentifier) + backend selector.
			Build(projectFile, additionalArgs: "-p:MauiActiveBackend=gtk");

			var testDll = IOPath.Combine(intermediateDirectory, "test.dll");
			AssertExists(testDll, nonEmpty: true);
			AssertTypeExists(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.GtkMarker");
			AssertTypeDoesNotExist(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.IosMarker");
		}

		// The neutral backend selector must not add backend files to a recognized
		// platform inner build. Android remains the only active platform here even
		// when MauiActiveBackend names GTK.
		[Fact]
		public void SingleProject_RecognizedTfmIgnoresNeutralBackendSelector()
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
				.WithAttribute("Include", "Platforms\\Gtk\\")
				.WithAttribute("BackendIdentity", "gtk"));
			project.Add(customMappings);

			WriteFile("Entry.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class Entry
{
	public static string Value => ""ok"";
}");

			WriteFile("Platforms\\Android\\AndroidMarker.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class AndroidMarker
{
	public static string Value => ""Android"";
}");

			WriteFile("Platforms\\Gtk\\GtkMarker.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class GtkMarker
{
	public static string Value => ""Gtk"";
}");

			AddSingleProjectTargetsImport(project);

			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);

			Build(projectFile, additionalArgs: "-p:_SingleProjectTestTargetPlatformIdentifier=android -p:MauiActiveBackend=gtk");

			var testDll = IOPath.Combine(intermediateDirectory, "test.dll");
			AssertExists(testDll, nonEmpty: true);
			AssertTypeExists(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.AndroidMarker");
			AssertTypeDoesNotExist(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.GtkMarker");
		}

		// A backend that is registered but not selected must be excluded: either
		// MauiActiveBackend names a different backend, or it is unset entirely.
		[Theory]
		[InlineData("cocoa")]
		[InlineData("")]
		public void SingleProject_NonMatchingBackendIsExcludedOnNeutralTfm(string activeBackend)
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
				.WithAttribute("Include", "Platforms\\Gtk\\")
				.WithAttribute("BackendIdentity", "gtk"));
			project.Add(customMappings);

			WriteFile("Entry.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class Entry
{
	public static string Value => ""ok"";
}");

			WriteFile("Platforms\\Gtk\\GtkMarker.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class GtkMarker
{
	public static string Value => ""Gtk"";
}");

			AddSingleProjectTargetsImport(project);

			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);

			var args = string.IsNullOrEmpty(activeBackend) ? "" : $"-p:MauiActiveBackend={activeBackend}";
			Build(projectFile, additionalArgs: args);

			var testDll = IOPath.Combine(intermediateDirectory, "test.dll");
			AssertExists(testDll, nonEmpty: true);
			AssertTypeDoesNotExist(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.GtkMarker");
		}

		// A backend may name a custom activation property instead of the default
		// MauiActiveBackend selector. The folder compiles only when the named
		// property equals the declared ActivationValue.
		[Theory]
		[InlineData("on", true)]
		[InlineData("off", false)]
		[InlineData("", false)]
		public void SingleProject_CustomActivationPropertyAndValueActivateBackend(string switchValue, bool shouldIncludeFooFile)
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
				.WithAttribute("Include", "Platforms\\Foo\\")
				.WithAttribute("BackendIdentity", "foo")
				.WithAttribute("ActivationProperty", "MyBackendSwitch")
				.WithAttribute("ActivationValue", "on"));
			project.Add(customMappings);

			WriteFile("Entry.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class Entry
{
	public static string Value => ""ok"";
}");

			WriteFile("Platforms\\Foo\\FooMarker.cs", @"
namespace Microsoft.Maui.Controls.Xaml.UnitTests;

public static class FooMarker
{
	public static string Value => ""Foo"";
}");

			AddSingleProjectTargetsImport(project);

			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);

			var args = string.IsNullOrEmpty(switchValue) ? "" : $"-p:MyBackendSwitch={switchValue}";
			Build(projectFile, additionalArgs: args);

			var testDll = IOPath.Combine(intermediateDirectory, "test.dll");
			AssertExists(testDll, nonEmpty: true);

			if (shouldIncludeFooFile)
				AssertTypeExists(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.FooMarker");
			else
				AssertTypeDoesNotExist(testDll, "Microsoft.Maui.Controls.Xaml.UnitTests.FooMarker");
		}
	}
}
