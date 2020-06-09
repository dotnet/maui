using Microsoft.Build.Locator;
using Mono.Cecil;
using NUnit.Framework;
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml.Linq;
using static Xamarin.Forms.MSBuild.UnitTests.MSBuildXmlExtensions;
using IOPath = System.IO.Path;

namespace Xamarin.Forms.MSBuild.UnitTests
{
	//This set of tests is for validating Xamarin.Forms.targets
	[TestFixture]
	[Category("LongRunning")]
	public class MSBuildTests
	{
		static readonly string [] references = {
			"mscorlib",
			"System",
			"Xamarin.Forms.Core.dll",
			"Xamarin.Forms.Xaml.dll",
		};

		class Xaml
		{
			const string XamarinFormsDefaultNamespace = "http://xamarin.com/schemas/2014/forms";
			const string XamarinFormsXNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";

			public static readonly string MainPage = $@"
				<ContentPage
					xmlns=""{XamarinFormsDefaultNamespace}""
					xmlns:x=""{XamarinFormsXNamespace}""
					x:Class=""Xamarin.Forms.Xaml.UnitTests.MainPage"">
					<Label x:Name=""label0""/>
				</ContentPage>";

			public static readonly string CustomView = $@"
				<ContentView
					xmlns=""{XamarinFormsDefaultNamespace}""
					xmlns:x=""{XamarinFormsXNamespace}""
					x:Class=""Xamarin.Forms.Xaml.UnitTests.CustomView"">
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

		string testDirectory;
		string tempDirectory;
		string intermediateDirectory;

		[SetUp]
		public void SetUp ()
		{
			testDirectory = TestContext.CurrentContext.TestDirectory;
			tempDirectory = IOPath.Combine (testDirectory, "temp", TestContext.CurrentContext.Test.Name);
			intermediateDirectory = IOPath.Combine (tempDirectory, "obj", "Debug");
			Directory.CreateDirectory (tempDirectory);

			//copy _Directory.Build.[props|targets] in test/
			var props = IOPath.Combine(testDirectory, "..", "..", "..", "MSBuild", "_Directory.Build.props");
			var targets = IOPath.Combine(testDirectory, "..", "..", "..", "MSBuild", "_Directory.Build.targets");
			if (!File.Exists(props))
			{
				//NOTE: VSTS may be running tests in a staging directory, so we can use an environment variable to find the source
				//https://docs.microsoft.com/en-us/vsts/build-release/concepts/definitions/build/variables?view=vsts&tabs=batch#buildsourcesdirectory
				var sourcesDirectory = Environment.GetEnvironmentVariable("BUILD_SOURCESDIRECTORY");
				if (!string.IsNullOrEmpty(sourcesDirectory)) {
					props = IOPath.Combine(sourcesDirectory, "Xamarin.Forms.Xaml.UnitTests", "MSBuild", "_Directory.Build.props");
					targets = IOPath.Combine(sourcesDirectory, "Xamarin.Forms.Xaml.UnitTests", "MSBuild", "_Directory.Build.targets");

					if (!File.Exists(props))
						Assert.Fail("Unable to find _Directory.Build.props at path: " + props);
				}
				else
					Assert.Fail("Unable to find _Directory.Build.props at path: " + props);

				Directory.CreateDirectory(IOPath.Combine(testDirectory, "..", "..", "..", "..",  ".nuspec"));
				foreach (var file in Directory.GetFiles(IOPath.Combine(sourcesDirectory, ".nuspec"), "*.targets"))
					File.Copy(file, IOPath.Combine(testDirectory, "..", "..", "..", "..", ".nuspec", IOPath.GetFileName(file)), true);
				foreach (var file in Directory.GetFiles(IOPath.Combine(sourcesDirectory, ".nuspec"), "*.props"))
					File.Copy(file, IOPath.Combine(testDirectory, "..", "..", "..", "..", ".nuspec", IOPath.GetFileName(file)), true);
				File.Copy(IOPath.Combine(sourcesDirectory, "Directory.Build.props"), IOPath.Combine(testDirectory, "..", "..", "..", "..", "Directory.Build.props"), true);
			}

			File.Copy(props, IOPath.Combine(tempDirectory, "Directory.Build.props"), true);
			File.Copy(targets, IOPath.Combine(tempDirectory, "Directory.Build.targets"), true);
		}

		[TearDown]
		public void TearDown ()
		{
			//NOTE: Windows can throw IOException: The process cannot access the file XYZ because it is being used by another process.
			//A simple retry-and-give-up approach should be good enough
			for (int i = 0; i < 3; i++) {
				try {
					if (Directory.Exists (tempDirectory)) {
						Directory.Delete (tempDirectory, true);
					}
					break; //Success
				} catch (IOException) {
					System.Threading.Thread.Sleep (100);
				}
			}
		}

		/// <summary>
		/// Creates a base csproj file for these unit tests
		/// </summary>
		/// <param name="sdkStyle">If true, uses a new SDK-style project</param>
		XElement NewProject (bool sdkStyle)
		{
			var project = NewElement ("Project");

			var propertyGroup = NewElement ("PropertyGroup");
			if (sdkStyle) {
				project.WithAttribute ("Sdk", "Microsoft.NET.Sdk");
				propertyGroup.Add (NewElement ("TargetFramework").WithValue ("netstandard2"));
				//NOTE: we don't want SDK-style projects to auto-add files, tests should be able to control this
				propertyGroup.Add(NewElement("EnableDefaultCompileItems").WithValue("False"));
				propertyGroup.Add(NewElement("EnableDefaultEmbeddedResourceItems").WithValue("False"));
				//NOTE: SDK-style output paths are different
				if (!intermediateDirectory.EndsWith ("netstandard2"))
					intermediateDirectory = IOPath.Combine (intermediateDirectory, "netstandard2");
			} else {
				propertyGroup.Add (NewElement ("Configuration").WithValue ("Debug"));
				propertyGroup.Add (NewElement ("Platform").WithValue ("AnyCPU"));
				propertyGroup.Add (NewElement ("OutputType").WithValue ("Library"));
				propertyGroup.Add (NewElement ("OutputPath").WithValue ("bin\\Debug"));
				propertyGroup.Add (NewElement ("TargetFrameworkVersion").WithValue ("v4.7"));
				propertyGroup.Add(NewElement("RootNamespace").WithValue("test"));
			}
			propertyGroup.Add(NewElement("_XFBuildTasksLocation").WithValue($"{testDirectory}\\"));

			project.Add (propertyGroup);

			var itemGroup = NewElement ("ItemGroup");
			foreach (var assembly in references) {
				var reference = NewElement ("Reference").WithAttribute ("Include", assembly);
				if (assembly.EndsWith (".dll", StringComparison.OrdinalIgnoreCase)) {
					reference.Add (NewElement ("HintPath").WithValue (IOPath.Combine ("..", "..", assembly)));
				} else if (sdkStyle) {
					//NOTE: SDK-style projects don't need system references
					continue;
				}
				itemGroup.Add (reference);
			}
			project.Add (itemGroup);

			//Let's enable XamlC assembly-wide
			project.Add (AddFile ("AssemblyInfo.cs", "Compile", "[assembly: Xamarin.Forms.Xaml.XamlCompilation (Xamarin.Forms.Xaml.XamlCompilationOptions.Compile)]"));

			//Add a single CSS file
			project.Add (AddFile ("Foo.css", "EmbeddedResource", Css.Foo));

			if (!sdkStyle)
				project.Add (NewElement ("Import").WithAttribute ("Project", @"$(MSBuildBinPath)\Microsoft.CSharp.targets"));
			return project;
		}

		XElement AddFile (string name, string buildAction, string contents)
		{
			var filePath = IOPath.Combine (tempDirectory, name.Replace ('\\', IOPath.DirectorySeparatorChar).Replace ('/', IOPath.DirectorySeparatorChar));
			Directory.CreateDirectory (IOPath.GetDirectoryName (filePath));
			File.WriteAllText (filePath, contents);
			var itemGroup = NewElement ("ItemGroup");
			itemGroup.Add (NewElement (buildAction).WithAttribute ("Include", name));
			return itemGroup;
		}

		string FindMSBuild ()
		{
			//On Windows we have to "find" MSBuild
			if (Environment.OSVersion.Platform == PlatformID.Win32NT) {
				foreach (var visualStudioInstance in MSBuildLocator.QueryVisualStudioInstances ().OrderByDescending (v => v.Version)) {
					return IOPath.Combine (visualStudioInstance.MSBuildPath, "MSBuild.exe");
				}
			}

			return "msbuild";
		}

		void RestoreIfNeeded (string projectFile, bool sdkStyle)
		{
			//If using an SDK-style project, we need to run the Restore target
			if (sdkStyle) {
				Build (projectFile, "Restore");
			}
		}

		string Build (string projectFile, string target = "Build", string additionalArgs = "", bool shouldSucceed = true)
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
			};

			var psi = new ProcessStartInfo {
				FileName = FindMSBuild (),
				Arguments = $"/v:normal /nologo {projectFile} /t:{target} /bl {additionalArgs}",
				CreateNoWindow = true,
				WindowStyle = ProcessWindowStyle.Hidden,
				UseShellExecute = false,
				RedirectStandardError = true,
				RedirectStandardOutput = true,
				WorkingDirectory = tempDirectory,
			};
			using (var p = new Process { StartInfo = psi }) {
				p.ErrorDataReceived += onData;
				p.OutputDataReceived += onData;

				p.Start ();
				p.BeginErrorReadLine ();
				p.BeginOutputReadLine ();
				p.WaitForExit ();
				if (shouldSucceed)
					Assert.AreEqual(0, p.ExitCode, "MSBuild exited with {0}", p.ExitCode);
				else
					Assert.AreNotEqual(0, p.ExitCode, "MSBuild exited with {0}", p.ExitCode);

				return builder.ToString();
			}
		}

		void AssertExists (string path, bool nonEmpty = false)
		{
			if (!File.Exists (path))
				Assert.Fail ($"{path} should exist!");

			if (nonEmpty && new FileInfo (path).Length == 0)
				Assert.Fail ($"{path} is empty!");
		}

		void AssertDoesNotExist (string path)
		{
			if (File.Exists (path))
				Assert.Fail ($"{path} should *not* exist!");
		}

		[Test]
		public void BuildAProject ([Values (false, true)] bool sdkStyle)
		{
			var project = NewProject (sdkStyle);
			project.Add (AddFile ("MainPage.xaml", "EmbeddedResource", Xaml.MainPage));
			var projectFile = IOPath.Combine (tempDirectory, "test.csproj");
			project.Save (projectFile);
			RestoreIfNeeded (projectFile, sdkStyle);
			Build (projectFile);

			AssertExists (IOPath.Combine (intermediateDirectory, "test.dll"), nonEmpty: true);
			AssertExists (IOPath.Combine (intermediateDirectory, "MainPage.xaml.g.cs"), nonEmpty: true);
			AssertExists (IOPath.Combine (intermediateDirectory, "Foo.css.g.cs"), nonEmpty: true);
			AssertExists (IOPath.Combine (intermediateDirectory, "XamlC.stamp"));
		}

		// Tests the XFXamlCValidateOnly=True MSBuild property
		[Test]
		public void ValidateOnly([Values(false, true)] bool sdkStyle)
		{
			var project = NewProject(sdkStyle);
			project.Add(AddFile("MainPage.xaml", "EmbeddedResource", Xaml.MainPage));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			RestoreIfNeeded(projectFile, sdkStyle);
			Build(projectFile, additionalArgs: "/p:XFXamlCValidateOnly=True");

			var testDll = IOPath.Combine(intermediateDirectory, "test.dll");
			AssertExists(testDll, nonEmpty: true);
			using (var assembly = AssemblyDefinition.ReadAssembly(testDll))
			{
				// XAML files should remain as EmbeddedResource
				var resources = assembly.MainModule.Resources.OfType<EmbeddedResource>().Select(e => e.Name).ToArray();
				CollectionAssert.Contains(resources, "test.MainPage.xaml");
			}
		}

		[Test]
		public void ValidateOnly_WithErrors([Values(false, true)] bool sdkStyle)
		{
			var project = NewProject(sdkStyle);
			project.Add(AddFile("MainPage.xaml", "EmbeddedResource", Xaml.MainPage.Replace ("</ContentPage>", "<NotARealThing/></ContentPage>")));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			RestoreIfNeeded(projectFile, sdkStyle);

			string log = Build(projectFile, additionalArgs: "/p:XFXamlCValidateOnly=True", shouldSucceed: false);
			StringAssert.Contains("MainPage.xaml(7,6): XamlC error XFC0000: Cannot resolve type \"NotARealThing\".", log);
		}

		/// <summary>
		/// Tests that XamlG and XamlC targets skip, as well as checking IncrementalClean doesn't delete generated files
		/// </summary>
		[Test]
		public void TargetsShouldSkip ([Values (false, true)] bool sdkStyle)
		{
			var project = NewProject (sdkStyle);
			project.Add (AddFile ("MainPage.xaml", "EmbeddedResource", Xaml.MainPage));
			var projectFile = IOPath.Combine (tempDirectory, "test.csproj");
			project.Save (projectFile);
			RestoreIfNeeded (projectFile, sdkStyle);
			Build (projectFile);

			var mainPageXamlG = IOPath.Combine (intermediateDirectory, "MainPage.xaml.g.cs");
			var fooCssG = IOPath.Combine (intermediateDirectory, "Foo.css.g.cs");
			var xamlCStamp = IOPath.Combine (intermediateDirectory, "XamlC.stamp");
			AssertExists (mainPageXamlG, nonEmpty: true);
			AssertExists (fooCssG, nonEmpty: true);
			AssertExists (xamlCStamp);

			var expectedXamlG = new FileInfo (mainPageXamlG).LastWriteTimeUtc;
			var expectdCssG = new FileInfo (fooCssG).LastWriteTimeUtc;
			var expectedXamlC = new FileInfo (xamlCStamp).LastWriteTimeUtc;

			//Build again
			Build (projectFile);
			AssertExists (mainPageXamlG, nonEmpty: true);
			AssertExists (xamlCStamp);

			var actualXamlG = new FileInfo (mainPageXamlG).LastWriteTimeUtc;
			var actualCssG = new FileInfo (fooCssG).LastWriteTimeUtc;
			var actualXamlC = new FileInfo (xamlCStamp).LastWriteTimeUtc;
			Assert.AreEqual (expectedXamlG, actualXamlG, $"Timestamps should match for {mainPageXamlG}.");
			Assert.AreEqual (expectdCssG, actualCssG, $"Timestamps should match for {fooCssG}.");
			Assert.AreEqual (expectedXamlC, actualXamlC, $"Timestamps should match for {xamlCStamp}.");
		}

		/// <summary>
		/// Checks that XamlG and XamlC files are cleaned
		/// </summary>
		[Test]
		public void Clean ([Values (false, true)] bool sdkStyle)
		{
			var project = NewProject (sdkStyle);
			project.Add (AddFile ("MainPage.xaml", "EmbeddedResource", Xaml.MainPage));
			var projectFile = IOPath.Combine (tempDirectory, "test.csproj");
			project.Save (projectFile);
			RestoreIfNeeded (projectFile, sdkStyle);
			Build (projectFile);

			var mainPageXamlG = IOPath.Combine (intermediateDirectory, "MainPage.xaml.g.cs");
			var fooCssG = IOPath.Combine (intermediateDirectory, "Foo.css.g.cs");
			var xamlCStamp = IOPath.Combine (intermediateDirectory, "XamlC.stamp");
			AssertExists (mainPageXamlG, nonEmpty: true);
			AssertExists (fooCssG, nonEmpty: true);
			AssertExists (xamlCStamp);

			//Clean
			Build (projectFile, "Clean");
			AssertDoesNotExist (mainPageXamlG);
			AssertDoesNotExist (fooCssG);
			AssertDoesNotExist (xamlCStamp);
		}

		[Test]
		public void LinkedFile ([Values (false, true)] bool sdkStyle)
		{
			var folder = IOPath.Combine (tempDirectory, "A", "B");
			Directory.CreateDirectory (folder);
			File.WriteAllText (IOPath.Combine (folder, "MainPage.xaml"), Xaml.MainPage);

			var project = NewProject (sdkStyle);
			var itemGroup = NewElement ("ItemGroup");
			var embeddedResource = NewElement ("EmbeddedResource").WithAttribute ("Include", @"A\B\MainPage.xaml");
			embeddedResource.Add (NewElement ("Link").WithValue (@"Pages\MainPage.xaml"));
			itemGroup.Add (embeddedResource);
			project.Add (itemGroup);
			var projectFile = IOPath.Combine (tempDirectory, "test.csproj");
			project.Save (projectFile);
			RestoreIfNeeded (projectFile, sdkStyle);
			Build (projectFile);

			AssertExists (IOPath.Combine (intermediateDirectory, "test.dll"), nonEmpty: true);
			AssertExists (IOPath.Combine (intermediateDirectory, "Pages", "MainPage.xaml.g.cs"), nonEmpty: true);
			AssertExists (IOPath.Combine (intermediateDirectory, "XamlC.stamp"));
		}

		//https://github.com/dotnet/project-system/blob/master/docs/design-time-builds.md
		//https://daveaglick.com/posts/running-a-design-time-build-with-msbuild-apis
		[Test]
		public void DesignTimeBuild ([Values (false/*, true */)] bool sdkStyle)
		{
			var project = NewProject (sdkStyle);
			project.Add (AddFile (@"Pages\MainPage.xaml", "EmbeddedResource", Xaml.MainPage));
			var projectFile = IOPath.Combine (tempDirectory, "test.csproj");
			project.Save (projectFile);
			RestoreIfNeeded (projectFile, sdkStyle);

			//NOTE: CompileDesignTime target only exists on Windows
			var target = Environment.OSVersion.Platform == PlatformID.Win32NT ? "CompileDesignTime" : "Compile";
			Build (projectFile, target, "/p:DesignTimeBuild=True /p:BuildingInsideVisualStudio=True /p:SkipCompilerExecution=True /p:ProvideCommandLineArgs=True");

			var assembly = IOPath.Combine (intermediateDirectory, "test.dll");
			var mainPageXamlG = IOPath.Combine (intermediateDirectory, "Pages", "MainPage.xaml.g.cs");
			var fooCssG = IOPath.Combine (intermediateDirectory, "Foo.css.g.cs");
			var xamlCStamp = IOPath.Combine (intermediateDirectory, "XamlC.stamp");

			//The assembly should not be compiled
			AssertDoesNotExist (assembly);
			AssertExists (mainPageXamlG, nonEmpty: true);
			AssertExists (fooCssG, nonEmpty: true);
			AssertDoesNotExist (xamlCStamp); //XamlC should be skipped

			var expectedXamlG = new FileInfo (mainPageXamlG).LastWriteTimeUtc;
			var expectedCssG = new FileInfo (fooCssG).LastWriteTimeUtc;

			//Build again, a full build
			Build (projectFile);
			AssertExists (assembly, nonEmpty: true);
			AssertExists (mainPageXamlG, nonEmpty: true);
			AssertExists (fooCssG, nonEmpty: true);
			AssertExists (xamlCStamp);

			var actualXamlG = new FileInfo (mainPageXamlG).LastWriteTimeUtc;
			var actualCssG = new FileInfo (fooCssG).LastWriteTimeUtc;
			Assert.AreEqual (expectedXamlG, actualXamlG, $"Timestamps should match for {mainPageXamlG}.");
			Assert.AreEqual (expectedCssG, actualCssG, $"Timestamps should match for {fooCssG}.");
		}

		//I believe the designer might invoke this target manually
		[Test]
		public void UpdateDesignTimeXaml ([Values (false, true)] bool sdkStyle)
		{
			var project = NewProject (sdkStyle);
			project.Add (AddFile (@"Pages\MainPage.xaml", "EmbeddedResource", Xaml.MainPage));
			var projectFile = IOPath.Combine (tempDirectory, "test.csproj");
			project.Save (projectFile);
			RestoreIfNeeded (projectFile, sdkStyle);
			Build (projectFile, "UpdateDesignTimeXaml");

			AssertExists (IOPath.Combine (intermediateDirectory, "Pages", "MainPage.xaml.g.cs"), nonEmpty: true);
			AssertDoesNotExist (IOPath.Combine (intermediateDirectory, "Foo.css.g.cs"));
			AssertDoesNotExist (IOPath.Combine (intermediateDirectory, "XamlC.stamp"));
		}

		[Test]
		public void AddNewFile ([Values (false, true)] bool sdkStyle)
		{
			var project = NewProject (sdkStyle);
			project.Add (AddFile ("MainPage.xaml", "EmbeddedResource", Xaml.MainPage));
			var projectFile = IOPath.Combine (tempDirectory, "test.csproj");
			project.Save (projectFile);
			RestoreIfNeeded (projectFile, sdkStyle);
			Build (projectFile);

			var mainPageXamlG = IOPath.Combine (intermediateDirectory, "MainPage.xaml.g.cs");
			var customViewXamlG = IOPath.Combine (intermediateDirectory, "CustomView.xaml.g.cs");
			var fooCssG = IOPath.Combine (intermediateDirectory, "Foo.css.g.cs");
			var xamlCStamp = IOPath.Combine (intermediateDirectory, "XamlC.stamp");
			AssertExists (mainPageXamlG, nonEmpty: true);
			AssertExists (xamlCStamp);

			var expectedXamlG = new FileInfo (mainPageXamlG).LastWriteTimeUtc;
			var expectedCssG = new FileInfo (fooCssG).LastWriteTimeUtc;
			var expectedXamlC = new FileInfo (xamlCStamp).LastWriteTimeUtc;

			//Build again, after adding a file, this triggers a full XamlG and XamlC -- *not* CssG
			project.Add (AddFile ("CustomView.xaml", "EmbeddedResource", Xaml.CustomView));
			project.Save (projectFile);
			Build (projectFile);
			AssertExists (mainPageXamlG, nonEmpty: true);
			AssertExists (customViewXamlG, nonEmpty: true);
			AssertExists (xamlCStamp);

			var actualXamlG = new FileInfo (mainPageXamlG).LastWriteTimeUtc;
			var actualCssG = new FileInfo (fooCssG).LastWriteTimeUtc;
			var actualXamlC = new FileInfo (xamlCStamp).LastWriteTimeUtc;
			var actualNewFile = new FileInfo (customViewXamlG).LastAccessTimeUtc;
			Assert.AreNotEqual (expectedXamlG, actualXamlG, $"Timestamps should *not* match for {mainPageXamlG}.");
			Assert.AreNotEqual (expectedXamlG, actualNewFile, $"Timestamps should *not* match for {customViewXamlG}.");
			Assert.AreEqual (expectedCssG, actualCssG, $"Timestamps should match for {fooCssG}.");
			Assert.AreNotEqual (expectedXamlC, actualXamlC, $"Timestamps should *not* match for {xamlCStamp}.");
		}

		[Test]
		public void TouchXamlFile ([Values (false, true)] bool sdkStyle)
		{
			var project = NewProject (sdkStyle);
			project.Add (AddFile ("MainPage.xaml", "EmbeddedResource", Xaml.MainPage));
			project.Add (AddFile ("CustomView.xaml", "EmbeddedResource", Xaml.CustomView));
			var projectFile = IOPath.Combine (tempDirectory, "test.csproj");
			project.Save (projectFile);
			RestoreIfNeeded (projectFile, sdkStyle);
			Build (projectFile);

			var mainPageXamlG = IOPath.Combine (intermediateDirectory, "MainPage.xaml.g.cs");
			var customViewXamlG = IOPath.Combine (intermediateDirectory, "CustomView.xaml.g.cs");
			var xamlCStamp = IOPath.Combine (intermediateDirectory, "XamlC.stamp");
			AssertExists (mainPageXamlG, nonEmpty: true);
			AssertExists (customViewXamlG, nonEmpty: true);
			AssertExists (xamlCStamp);

			var expectedMainPageXamlG = new FileInfo (mainPageXamlG).LastWriteTimeUtc;
			var expectedCustomViewXamlG = new FileInfo (customViewXamlG).LastWriteTimeUtc;
			var expectedXamlC = new FileInfo (xamlCStamp).LastWriteTimeUtc;

			//Build again, after modifying the timestamp on a Xaml file, should trigger a partial XamlG and full XamlC
			//https://github.com/xamarin/xamarin-android/blob/61851599fb1999964bd200ec1c373b6e395933f3/src/Xamarin.Android.Build.Tasks/Utilities/MonoAndroidHelper.cs#L342
			File.SetLastWriteTimeUtc (customViewXamlG, expectedCustomViewXamlG.AddDays (1));
			File.SetLastAccessTimeUtc (customViewXamlG, expectedCustomViewXamlG.AddDays (1));
			Build (projectFile);
			AssertExists (mainPageXamlG, nonEmpty: true);
			AssertExists (customViewXamlG, nonEmpty: true);
			AssertExists (xamlCStamp);

			var actualMainPageXamlG = new FileInfo (mainPageXamlG).LastWriteTimeUtc;
			var actualCustomViewXamlG = new FileInfo (customViewXamlG).LastAccessTimeUtc;
			var actualXamlC = new FileInfo (xamlCStamp).LastWriteTimeUtc;
			Assert.AreEqual (expectedMainPageXamlG, actualMainPageXamlG, $"Timestamps should match for {mainPageXamlG}.");
			Assert.AreNotEqual (expectedMainPageXamlG, actualCustomViewXamlG, $"Timestamps should *not* match for {actualCustomViewXamlG}.");
			Assert.AreNotEqual (expectedXamlC, actualXamlC, $"Timestamps should *not* match for {xamlCStamp}.");
		}

		[Test]
		public void RandomXml ([Values (false, true)] bool sdkStyle)
		{
			var project = NewProject (sdkStyle);
			project.Add (AddFile ("MainPage.xaml", "EmbeddedResource", "<xml></xml>"));
			var projectFile = IOPath.Combine (tempDirectory, "test.csproj");
			project.Save (projectFile);
			RestoreIfNeeded (projectFile, sdkStyle);
			Build (projectFile);

			AssertExists (IOPath.Combine (intermediateDirectory, "test.dll"), nonEmpty: true);
			AssertExists (IOPath.Combine (intermediateDirectory, "MainPage.xaml.g.cs"));
			AssertExists (IOPath.Combine (intermediateDirectory, "XamlC.stamp"));
		}

		[Test]
		public void InvalidXml([Values(false, true)] bool sdkStyle)
		{
			var project = NewProject(sdkStyle);
			project.Add(AddFile("MainPage.xaml", "EmbeddedResource", "notxmlatall"));
			var projectFile = IOPath.Combine(tempDirectory, "test.csproj");
			project.Save(projectFile);
			RestoreIfNeeded(projectFile, sdkStyle);
			Assert.Throws<AssertionException>(() => Build(projectFile));
		}

		[Test]
		public void RandomEmbeddedResource ([Values (false, true)] bool sdkStyle)
		{
			var project = NewProject (sdkStyle);
			project.Add (AddFile ("MainPage.txt", "EmbeddedResource", "notxmlatall"));
			var projectFile = IOPath.Combine (tempDirectory, "test.csproj");
			project.Save (projectFile);
			RestoreIfNeeded (projectFile, sdkStyle);
			Build (projectFile);

			AssertExists (IOPath.Combine (intermediateDirectory, "test.dll"), nonEmpty: true);
			AssertDoesNotExist (IOPath.Combine (intermediateDirectory, "MainPage.txt.g.cs"));
			AssertExists (IOPath.Combine (intermediateDirectory, "XamlC.stamp"));
		}
	}
}
