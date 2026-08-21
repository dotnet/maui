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
		var defaultExtensionsPath = GetProjectProperty(projectFile, "MSBuildExtensionsPath");
		var extensionsPathWithoutSeparator = defaultExtensionsPath.TrimEnd(
			Path.DirectorySeparatorChar,
			Path.AltDirectorySeparatorChar);
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
			$"{withoutSeparator.ExtensionsPath}../../sdk-manifests/{withoutSeparator.ManifestBand}",
			withoutSeparator.ManifestDirectory);
		Assert.Equal(
			$"{withoutSeparator.ExtensionsPath}../../dotnet",
			withoutSeparator.DotNetPath);
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
		var propertyArgument = extensionsPath is null
			? string.Empty
			: $" -p:MSBuildExtensionsPath=\"{extensionsPath}\"";
		var output = DotnetInternal.RunForOutput(
			"msbuild",
			$"\"{projectFile}\" -nologo -verbosity:quiet -getProperty:{propertyName}{propertyArgument}",
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
