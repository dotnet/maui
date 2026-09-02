namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class DotNetProjectTests
{
	readonly ITestOutputHelper _output;

	public DotNetProjectTests(ITestOutputHelper output)
	{
		_output = output;
	}

	[Fact]
	public void WorkloadInstallPathsAreIndependentOfTrailingSeparator()
	{
		var projectFile = Path.Combine(
			TestEnvironment.GetMauiDirectory(),
			"src",
			"DotNet",
			"DotNet.csproj");
		var sdkVersion = DotnetInternal.RunForOutput(
			new[] { "--version" },
			out var exitCode,
			timeoutInSeconds: 60,
			output: _output).Trim();
		Assert.True(exitCode == 0, "Unable to determine the .NET SDK version.");
		Assert.False(string.IsNullOrEmpty(sdkVersion), "The .NET SDK version was empty.");

		var extensionsPathWithoutSeparator = Path.Combine(
			TestEnvironment.GetMauiDirectory(),
			".dotnet",
			"sdk",
			sdkVersion);
		var extensionsPathWithSeparator = extensionsPathWithoutSeparator + Path.DirectorySeparatorChar;

		var withoutSeparator = GetWorkloadPaths(projectFile, extensionsPathWithoutSeparator);
		var withSeparator = GetWorkloadPaths(projectFile, extensionsPathWithSeparator);

		Assert.Equal(withSeparator, withoutSeparator);
		Assert.True(
			withoutSeparator.ExtensionsPath.EndsWith(
				Path.DirectorySeparatorChar.ToString(),
				StringComparison.Ordinal) ||
			withoutSeparator.ExtensionsPath.EndsWith(
				Path.AltDirectorySeparatorChar.ToString(),
				StringComparison.Ordinal));
		Assert.Equal(
			Path.GetFullPath(Path.Combine(
				withoutSeparator.ExtensionsPath,
				"..",
				"..",
				"sdk-manifests",
				withoutSeparator.ManifestBand)),
			Path.GetFullPath(withoutSeparator.ManifestDirectory));
		Assert.Equal(
			Path.GetFullPath(Path.Combine(
				withoutSeparator.ExtensionsPath,
				"..",
				"..",
				"dotnet")),
			Path.GetFullPath(withoutSeparator.DotNetPath));
	}

	(string ExtensionsPath, string ManifestDirectory, string DotNetPath, string ManifestBand) GetWorkloadPaths(
		string projectFile,
		string extensionsPath)
	{
		return (
			GetProjectProperty(projectFile, "_NormalizedMSBuildExtensionsPath", extensionsPath),
			GetProjectProperty(projectFile, "_WorkloadManifestInstallDirectory", extensionsPath),
			GetProjectProperty(projectFile, "_WorkloadInstallDotNetPath", extensionsPath),
			GetProjectProperty(projectFile, "DotNetSdkManifestsFolder", extensionsPath));
	}

	string GetProjectProperty(string projectFile, string propertyName, string? extensionsPath = null)
	{
		var arguments = new List<string>
		{
			"msbuild",
			projectFile,
			"-nologo",
			"-verbosity:quiet",
			$"-getProperty:{propertyName}",
		};
		if (extensionsPath is not null)
			arguments.Add($"-p:MSBuildExtensionsPath={extensionsPath}");

		var output = DotnetInternal.RunForOutput(
			arguments,
			out var exitCode,
			timeoutInSeconds: 60,
			output: _output);

		Assert.True(exitCode == 0, $"MSBuild property evaluation failed:{Environment.NewLine}{output}");

		var value = output
			.Split(new[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries)
			.LastOrDefault()?
			.Trim();

		if (string.IsNullOrEmpty(value))
			throw new XunitException($"MSBuild property '{propertyName}' was empty.");

		return value;
	}
}
