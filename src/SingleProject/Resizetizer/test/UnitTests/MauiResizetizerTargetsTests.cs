using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;
using Xunit.Abstractions;

namespace Microsoft.Maui.Resizetizer.Tests
{
	public class MauiResizetizerTargetsTests : BaseTest
	{
		public MauiResizetizerTargetsTests(ITestOutputHelper output)
			: base(output)
		{
		}

		[Fact]
		public void ProcessMauiSplashScreensSelectsFirstSplashForThemedAppleMetadata()
		{
			var targetFile = Path.Combine(
				FindRepositoryRoot(),
				"src",
				"SingleProject",
				"Resizetizer",
				"src",
				"nuget",
				"buildTransitive",
				"Microsoft.Maui.Resizetizer.After.targets");
			var metadataElements = GetSplashMetadataElements(targetFile);
			var projectFile = Path.Combine(DestinationDirectory, "SelectFirstSplash.proj");
			Directory.CreateDirectory(DestinationDirectory);

			File.WriteAllText(projectFile,
				$$"""
				<Project>
				  <PropertyGroup>
				    <_ResizetizerIsiOSApp>True</_ResizetizerIsiOSApp>
				    <SupportedOSPlatformVersion>14.0</SupportedOSPlatformVersion>
				  </PropertyGroup>
				  <ItemGroup>
				    <MauiSplashScreen Include="splash.png" DarkFile="first-dark.png" />
				    <MauiSplashScreen Include="splash.png" Color="#ffffff" />
				  </ItemGroup>
				  <Target Name="Validate">
				{{metadataElements}}
				    <Message Importance="high" Text="First=$(_MauiFirstSplashScreenIdentity); DarkFile=$(_MauiFirstSplashScreenDarkFile); FirstColor=$(_MauiFirstSplashScreenColor); Themed=$(_MauiHasThemedSplashScreen); Color=$(_MauiLaunchScreenColorName)" />
				    <Error Condition="'$(_MauiFirstSplashScreenIdentity)' != 'splash.png'" Text="Expected the first splash screen identity, got '$(_MauiFirstSplashScreenIdentity)'." />
				    <Error Condition="'$(_MauiFirstSplashScreenDarkFile)' != 'first-dark.png'" Text="Expected dark metadata from the first duplicate splash screen item, got '$(_MauiFirstSplashScreenDarkFile)'." />
				    <Error Condition="'$(_MauiFirstSplashScreenColor)' != ''" Text="Expected color metadata from the second duplicate splash screen item to be ignored, got '$(_MauiFirstSplashScreenColor)'." />
				    <Error Condition="'$(_MauiHasThemedSplashScreen)' != 'true'" Text="Expected the first splash screen dark metadata to enable themed Apple splash screens." />
				    <Error Condition="'$(_MauiLaunchScreenColorName)' != ''" Text="Expected color metadata from later splash screen items to be ignored, got '$(_MauiLaunchScreenColorName)'." />
				    <Error Condition="'$(_MauiShouldUseThemedAppleSplashScreen)' != 'true'" Text="Expected themed Apple splash screen generation to be enabled." />
				  </Target>
				</Project>
				""");

			var output = RunDotnetMSBuild(projectFile);

			Assert.Contains("First=splash.png; DarkFile=first-dark.png; FirstColor=; Themed=true; Color=", output, StringComparison.Ordinal);
		}

		[Fact]
		public void ProcessMauiSplashScreensWarnsWhenThemedAppleSplashIsUnsupported()
		{
			var targetFile = Path.Combine(
				FindRepositoryRoot(),
				"src",
				"SingleProject",
				"Resizetizer",
				"src",
				"nuget",
				"buildTransitive",
				"Microsoft.Maui.Resizetizer.After.targets");
			var metadataAndWarnings = GetSplashMetadataElements(targetFile, includeWarnings: true);
			var projectFile = Path.Combine(DestinationDirectory, "UnsupportedThemedSplash.proj");
			Directory.CreateDirectory(DestinationDirectory);

			File.WriteAllText(projectFile,
				$$"""
				<Project>
				  <PropertyGroup>
				    <_ResizetizerIsiOSSpecificApp>True</_ResizetizerIsiOSSpecificApp>
				    <SupportedOSPlatformVersion>13.0</SupportedOSPlatformVersion>
				  </PropertyGroup>
				  <ItemGroup>
				    <MauiSplashScreen Include="first.png" DarkFile="first-dark.png" />
				  </ItemGroup>
				  <Target Name="Validate">
				{{metadataAndWarnings}}
				    <Error Condition="'$(_MauiHasThemedSplashScreen)' != 'true'" Text="Expected themed splash metadata to be detected." />
				    <Error Condition="'$(_MauiShouldUseThemedAppleSplashScreen)' == 'true'" Text="Expected themed Apple splash screen generation to stay disabled." />
				  </Target>
				</Project>
				""");

			var output = RunDotnetMSBuild(projectFile);

			Assert.Contains("Themed MauiSplashScreen assets require iOS or iPadOS 14.0 or later", output, StringComparison.Ordinal);
		}

		static string GetSplashMetadataElements(string targetFile, bool includeWarnings = false)
		{
			var doc = XDocument.Load(targetFile);
			var processSplashScreens = Assert.Single(doc.Root.Elements().Where(e => e.Name.LocalName == "Target" && e.Attribute("Name")?.Value == "ProcessMauiSplashScreens"));
			var propertyGroups = processSplashScreens
				.Elements()
				.Where(e => e.Name.LocalName == "PropertyGroup")
				.ToArray();

			var firstSplashMetadata = Assert.Single(propertyGroups.Where(e => e.Elements().Any(child => child.Name.LocalName == "_MauiHasSplashScreens")));
			var themedSplashMetadata = Assert.Single(propertyGroups.Where(e => e.Elements().Any(child => child.Name.LocalName == "_MauiHasThemedSplashScreen")));

			var elements = includeWarnings
				? new[] { firstSplashMetadata, themedSplashMetadata }.Concat(processSplashScreens.Elements().Where(e => e.Name.LocalName == "Warning"))
				: new[] { firstSplashMetadata, themedSplashMetadata };

			return string.Join(Environment.NewLine, elements.Select(e => StripNamespace(e).ToString(SaveOptions.DisableFormatting)));
		}

		static XElement StripNamespace(XElement element) =>
			new(
				element.Name.LocalName,
				element.Attributes().Where(a => !a.IsNamespaceDeclaration).Select(a => new XAttribute(a.Name.LocalName, a.Value)),
				element.Nodes().Select(n => n is XElement child ? StripNamespace(child) : n));

		string RunDotnetMSBuild(string projectFile)
		{
			var startInfo = new ProcessStartInfo
			{
				FileName = GetDotNetHost(),
				WorkingDirectory = DestinationDirectory,
				UseShellExecute = false,
				RedirectStandardOutput = true,
				RedirectStandardError = true,
			};
			startInfo.ArgumentList.Add("msbuild");
			startInfo.ArgumentList.Add(projectFile);
			startInfo.ArgumentList.Add("-t:Validate");
			startInfo.ArgumentList.Add("-nologo");
			startInfo.ArgumentList.Add("-v:minimal");

			using var process = Process.Start(startInfo);
			Assert.NotNull(process);

			var outputTask = process.StandardOutput.ReadToEndAsync();
			var errorTask = process.StandardError.ReadToEndAsync();
			process.WaitForExit();
			var output = outputTask.GetAwaiter().GetResult();
			var error = errorTask.GetAwaiter().GetResult();

			Output.WriteLine(output);
			Output.WriteLine(error);

			Assert.Equal(0, process.ExitCode);

			return output + error;
		}

		static string GetDotNetHost()
		{
			var dotnetHost = Environment.GetEnvironmentVariable("DOTNET_HOST_PATH");

			return !string.IsNullOrWhiteSpace(dotnetHost) && File.Exists(dotnetHost)
				? dotnetHost
				: "dotnet";
		}

		static string FindRepositoryRoot()
		{
			var directory = new DirectoryInfo(AppContext.BaseDirectory);

			while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "Microsoft.Maui.sln")))
				directory = directory.Parent;

			return directory?.FullName ?? throw new DirectoryNotFoundException("Could not find the repository root.");
		}
	}
}
