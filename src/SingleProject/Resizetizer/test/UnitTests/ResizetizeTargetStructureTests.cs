using System;
using System.Diagnostics;
using System.IO;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class ResizetizeTargetStructureTests : BaseTest
	{
		readonly string _repoRoot;
		readonly string _targetsFilePath;
		readonly string _taskAssemblyPath;

		public ResizetizeTargetStructureTests(ITestOutputHelper output)
			: base(output)
		{
			_repoRoot = FindRepoRoot(Directory.GetCurrentDirectory());
			_targetsFilePath = Path.Combine(_repoRoot, "src", "SingleProject", "Resizetizer", "src", "nuget", "buildTransitive", "Microsoft.Maui.Resizetizer.After.targets");
			_taskAssemblyPath = Path.Combine(_repoRoot, ".buildtasks", "Microsoft.Maui.Resizetizer.dll");

			Assert.True(File.Exists(_targetsFilePath), $"Targets file not found at: {_targetsFilePath}");
			Assert.True(File.Exists(_taskAssemblyPath), $"Task assembly not found at: {_taskAssemblyPath}");
		}

		[Fact]
		public void Fonts_AreRegistered_OnFirstAndIncrementalBuilds_WhileTargetRemainsIncremental()
		{
			var projectDir = CreateProject(includeSplashScreen: false);

			RunMsbuild(projectDir, "_ComputeAndroidAssetsPaths", "fonts-build-1.log");
			Assert.Equal("1", ReadOutputValue(projectDir, "font-count.txt"));

			var secondBuildLog = RunMsbuild(projectDir, "_ComputeAndroidAssetsPaths", "fonts-build-2.log");
			Assert.Equal("1", ReadOutputValue(projectDir, "font-count.txt"));
			Assert.Contains("Skipping target \"ProcessMauiFonts\"", secondBuildLog, StringComparison.Ordinal);
		}

		[Fact]
		public void SplashScreens_AreRegistered_OnFirstAndIncrementalBuilds_WhileTargetRemainsIncremental()
		{
			var projectDir = CreateProject(includeSplashScreen: true);

			RunMsbuild(projectDir, "CheckSplashAssets", "splash-build-1.log");
			Assert.Equal("1", ReadOutputValue(projectDir, "splash-count.txt"));

			var secondBuildLog = RunMsbuild(projectDir, "CheckSplashAssets", "splash-build-2.log");
			Assert.Equal("1", ReadOutputValue(projectDir, "splash-count.txt"));
			Assert.Contains("Skipping target \"ProcessMauiSplashScreens\"", secondBuildLog, StringComparison.Ordinal);
		}

		string CreateProject(bool includeSplashScreen)
		{
			var projectDirName = includeSplashScreen
				? $"splash-{DateTime.UtcNow.Ticks}"
				: $"fonts-{DateTime.UtcNow.Ticks}";
			var projectDir = Path.Combine(DestinationDirectory, projectDirName);
			Directory.CreateDirectory(projectDir);

			var fontPath = Path.Combine(projectDir, "font.ttf");
			// ProcessMauiFonts copies files without validating font internals, so a placeholder file is sufficient.
			File.WriteAllBytes(fontPath, Array.Empty<byte>());

			var splashItem = string.Empty;
			if (includeSplashScreen)
			{
				var splashSource = Path.Combine(_repoRoot, "src", "SingleProject", "Resizetizer", "test", "UnitTests", "images", "camera.png");
				Assert.True(File.Exists(splashSource), $"Splash source image not found at: {splashSource}");
				File.Copy(splashSource, Path.Combine(projectDir, "splash.png"), overwrite: true);
				splashItem = "    <MauiSplashScreen Include=\"splash.png\" />" + Environment.NewLine;
			}

			var projectContents =
$@"<Project DefaultTargets=""VerifyAssets"" xmlns=""http://schemas.microsoft.com/developer/msbuild/2003"">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <AndroidApplication>True</AndroidApplication>
    <OutputType>Exe</OutputType>
    <IntermediateOutputPath>obj\</IntermediateOutputPath>
    <EnableMauiAssetProcessing>false</EnableMauiAssetProcessing>
  </PropertyGroup>
  <ItemGroup>
    <MauiFont Include=""font.ttf"" />
{splashItem}  </ItemGroup>
  <Import Project=""{_targetsFilePath}"" />
  <Target Name=""_ComputeAndroidAssetsPaths"">
    <WriteLinesToFile File=""$(IntermediateOutputPath)font-count.txt"" Lines=""@(AndroidAsset->Count())"" Overwrite=""true"" />
  </Target>
  <Target Name=""CheckSplashAssets"" DependsOnTargets=""_CollectMauiSplashScreens"">
    <WriteLinesToFile File=""$(IntermediateOutputPath)splash-count.txt"" Lines=""@(LibraryResourceDirectories->Count())"" Overwrite=""true"" />
  </Target>
  <Target Name=""VerifyAssets"" DependsOnTargets=""_ComputeAndroidAssetsPaths;CheckSplashAssets"" />
</Project>";

			File.WriteAllText(Path.Combine(projectDir, "Test.proj"), projectContents, System.Text.Encoding.UTF8);
			return projectDir;
		}

		string RunMsbuild(string projectDir, string target, string logFileName)
		{
			var projectFile = Path.Combine(projectDir, "Test.proj");
			var logFile = Path.Combine(projectDir, logFileName);
			var args =
				$"msbuild \"{projectFile}\" " +
				$"/t:{target} " +
				$"/v:diag " +
				$"/p:_ResizetizerTaskAssemblyName=\"{_taskAssemblyPath}\" " +
				"/p:_ResizetizerIsAndroidApp=True " +
				"/p:_ResizetizerIsCompatibleApp=True " +
				"/p:ResizetizerPlatformType=android " +
				"/p:TargetFrameworkIdentifier=.NETCoreApp";

			var startInfo = new ProcessStartInfo("dotnet", args)
			{
				WorkingDirectory = projectDir,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
				UseShellExecute = false,
				CreateNoWindow = true
			};

			using var process = Process.Start(startInfo);
			var standardOutput = process!.StandardOutput.ReadToEnd();
			var standardError = process.StandardError.ReadToEnd();
			process.WaitForExit();

			var output = standardOutput + Environment.NewLine + standardError;
			File.WriteAllText(logFile, output, System.Text.Encoding.UTF8);

			Assert.True(process.ExitCode == 0, $"dotnet msbuild failed with exit code {process.ExitCode}.{Environment.NewLine}{output}");

			return output;
		}

		static string ReadOutputValue(string projectDir, string fileName)
		{
			var outputFile = Path.Combine(projectDir, "obj", fileName);
			Assert.True(File.Exists(outputFile), $"Expected output file not found: {outputFile}");
			return File.ReadAllText(outputFile).Trim();
		}

		static string FindRepoRoot(string startDirectory)
		{
			var current = new DirectoryInfo(startDirectory);

			while (current is not null)
			{
				var marker = Path.Combine(current.FullName, "src", "SingleProject", "Resizetizer");
				if (Directory.Exists(marker))
					return current.FullName;

				current = current.Parent;
			}

			throw new DirectoryNotFoundException($"Unable to locate repository root from: {startDirectory}");
		}
	}
}
