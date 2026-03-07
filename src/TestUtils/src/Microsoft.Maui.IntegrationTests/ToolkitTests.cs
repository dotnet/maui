namespace Microsoft.Maui.IntegrationTests;

/// <summary>
/// Integration tests that verify .NET MAUI builds successfully with popular third-party toolkit packages.
/// 
/// These tests help catch regressions where MAUI changes break compatibility with widely-used community packages
/// such as CommunityToolkit.Maui and Syncfusion.Maui.Toolkit. By building template projects with these packages,
/// we ensure that MAUI's build system, MSBuild tasks, and SDK remain compatible with the broader MAUI ecosystem.
///
/// Tests follow the same pattern as SampleTests.cs - creating projects from templates, adding package references,
/// and building to ensure no breaking changes have been introduced.
///
/// See also: https://github.com/dotnet/maui/pull/34047
/// </summary>
[Trait("Category", "Build")]
public class ToolkitTests : BaseTemplateTests
{
	public ToolkitTests(IntegrationTestFixture fixture, ITestOutputHelper output) : base(fixture, output) { }

	/// <summary>
	/// Tests that a MAUI app can build successfully with CommunityToolkit.Maui package installed.
	/// This catches regressions where MAUI changes break compatibility with the Community Toolkit.
	/// </summary>
	[Theory]
	[InlineData("maui", DotNetCurrent, "Debug")]
	[InlineData("maui", DotNetCurrent, "Release")]
	public void BuildWithCommunityToolkit(string id, string framework, string config)
	{
		SetTestIdentifier(id, framework, config, "CommunityToolkit");
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		// Create project from template
		Assert.True(DotnetInternal.New(id, projectDir, framework, output: _output),
			$"Unable to create template {id}. Check test output for errors.");

		// Add CommunityToolkit.Maui package reference
		var communityToolkitVersion = GetPackageVersion("CommunityToolkitMauiPackageVersion");
		FileUtilities.ReplaceInFile(projectFile,
			"</Project>",
			$"""
			  <ItemGroup>
			    <PackageReference Include="CommunityToolkit.Maui" Version="{communityToolkitVersion}" />
			  </ItemGroup>
			</Project>
			""");

		// Update MauiProgram.cs to use the toolkit
		var mauiProgramFile = Path.Combine(projectDir, "MauiProgram.cs");
		if (File.Exists(mauiProgramFile))
		{
			FileUtilities.ReplaceInFile(mauiProgramFile,
				"using Microsoft.Extensions.Logging;",
				"""
				using Microsoft.Extensions.Logging;
				using CommunityToolkit.Maui;
				""");
			
			FileUtilities.ReplaceInFile(mauiProgramFile,
				".UseMauiApp<App>()",
				"""
				.UseMauiApp<App>()
					.UseMauiCommunityToolkit()
				""");
		}

		// Build the project
		Assert.True(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Project {Path.GetFileName(projectFile)} with CommunityToolkit.Maui failed to build. Check test output/attachments for errors.");
	}

	/// <summary>
	/// Tests that a MAUI app can build successfully with Syncfusion.Maui.Toolkit package installed.
	/// This catches regressions where MAUI changes break compatibility with Syncfusion Toolkit.
	/// </summary>
	[Theory]
	[InlineData("maui", DotNetCurrent, "Debug")]
	[InlineData("maui", DotNetCurrent, "Release")]
	public void BuildWithSyncfusionToolkit(string id, string framework, string config)
	{
		SetTestIdentifier(id, framework, config, "SyncfusionToolkit");
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		// Create project from template
		Assert.True(DotnetInternal.New(id, projectDir, framework, output: _output),
			$"Unable to create template {id}. Check test output for errors.");

		// Add Syncfusion.Maui.Toolkit package reference
		var syncfusionVersion = GetPackageVersion("SyncfusionMauiToolkitPackageVersion");
		FileUtilities.ReplaceInFile(projectFile,
			"</Project>",
			$"""
			  <ItemGroup>
			    <PackageReference Include="Syncfusion.Maui.Toolkit" Version="{syncfusionVersion}" />
			  </ItemGroup>
			</Project>
			""");

		// Update MauiProgram.cs to register Syncfusion handlers
		var mauiProgramFile = Path.Combine(projectDir, "MauiProgram.cs");
		if (File.Exists(mauiProgramFile))
		{
			FileUtilities.ReplaceInFile(mauiProgramFile,
				"using Microsoft.Extensions.Logging;",
				"""
				using Microsoft.Extensions.Logging;
				using Syncfusion.Maui.Toolkit.Hosting;
				""");
			
			FileUtilities.ReplaceInFile(mauiProgramFile,
				".UseMauiApp<App>()",
				"""
				.UseMauiApp<App>()
					.ConfigureSyncfusionToolkit()
				""");
		}

		// Build the project
		Assert.True(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Project {Path.GetFileName(projectFile)} with Syncfusion.Maui.Toolkit failed to build. Check test output/attachments for errors.");
	}

	/// <summary>
	/// Tests that a MAUI app can build successfully with both CommunityToolkit.Maui and Syncfusion.Maui.Toolkit packages.
	/// This catches regressions where multiple popular toolkits conflict or where MAUI changes break multi-toolkit scenarios.
	/// </summary>
	[Theory]
	[InlineData("maui", DotNetCurrent, "Debug")]
	[InlineData("maui", DotNetCurrent, "Release")]
	public void BuildWithMultipleToolkits(string id, string framework, string config)
	{
		SetTestIdentifier(id, framework, config, "MultipleToolkits");
		var projectDir = TestDirectory;
		var projectFile = Path.Combine(projectDir, $"{Path.GetFileName(projectDir)}.csproj");

		// Create project from template
		Assert.True(DotnetInternal.New(id, projectDir, framework, output: _output),
			$"Unable to create template {id}. Check test output for errors.");

		// Add both toolkit package references
		var communityToolkitVersion = GetPackageVersion("CommunityToolkitMauiPackageVersion");
		var syncfusionVersion = GetPackageVersion("SyncfusionMauiToolkitPackageVersion");
		FileUtilities.ReplaceInFile(projectFile,
			"</Project>",
			$"""
			  <ItemGroup>
			    <PackageReference Include="CommunityToolkit.Maui" Version="{communityToolkitVersion}" />
			    <PackageReference Include="Syncfusion.Maui.Toolkit" Version="{syncfusionVersion}" />
			  </ItemGroup>
			</Project>
			""");

		// Update MauiProgram.cs to use both toolkits
		var mauiProgramFile = Path.Combine(projectDir, "MauiProgram.cs");
		if (File.Exists(mauiProgramFile))
		{
			FileUtilities.ReplaceInFile(mauiProgramFile,
				"using Microsoft.Extensions.Logging;",
				"""
				using Microsoft.Extensions.Logging;
				using CommunityToolkit.Maui;
				using Syncfusion.Maui.Toolkit.Hosting;
				""");
			
			FileUtilities.ReplaceInFile(mauiProgramFile,
				".UseMauiApp<App>()",
				"""
				.UseMauiApp<App>()
					.UseMauiCommunityToolkit()
					.ConfigureSyncfusionToolkit()
				""");
		}

		// Build the project
		Assert.True(DotnetInternal.Build(projectFile, config, properties: BuildProps, msbuildWarningsAsErrors: true, output: _output),
			$"Project {Path.GetFileName(projectFile)} with multiple toolkits failed to build. Check test output/attachments for errors.");
	}

	/// <summary>
	/// Helper method to read package versions from Versions.props
	/// </summary>
	private string GetPackageVersion(string propertyName)
	{
		var versionsPropsPath = Path.Combine(TestEnvironment.GetMauiDirectory(), "eng", "Versions.props");
		var versionsProps = System.Xml.Linq.XDocument.Load(versionsPropsPath);
		var version = versionsProps.Descendants(propertyName).FirstOrDefault()?.Value;
		
		if (string.IsNullOrEmpty(version))
			throw new Exception($"Could not find {propertyName} in Versions.props");
		
		return version;
	}
}
