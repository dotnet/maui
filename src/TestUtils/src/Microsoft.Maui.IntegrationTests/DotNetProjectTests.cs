using System.Text.Json;

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
		var globalJson = JsonDocument.Parse(File.ReadAllText(Path.Combine(
			TestEnvironment.GetMauiDirectory(),
			"global.json")));
		var sdkVersion = globalJson.RootElement
			.GetProperty("tools")
			.GetProperty("dotnet")
			.GetString();
		Assert.False(string.IsNullOrEmpty(sdkVersion), "The .NET SDK version was empty.");

		var extensionsPathWithoutSeparator = Path.Combine(
			TestEnvironment.GetMauiDirectory(),
			".dotnet",
			"sdk",
			sdkVersion);
		Assert.True(
			Directory.Exists(extensionsPathWithoutSeparator),
			$"The repo-local .NET SDK directory does not exist: {extensionsPathWithoutSeparator}");
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
		var resultFile = Path.Combine(
			Path.GetTempPath(),
			$"maui-msbuild-properties-{Guid.NewGuid():N}.json");

		try
		{
			var arguments =
				$"\"{projectFile}\" -nologo -verbosity:quiet " +
				$"-getProperty:_NormalizedMSBuildExtensionsPath,_WorkloadManifestInstallDirectory,_WorkloadInstallDotNetPath,DotNetSdkManifestsFolder " +
				$"-getResultOutputFile:\"{resultFile}\" " +
				$"-p:MSBuildExtensionsPath=\"{extensionsPath}\"";
			var output = DotnetInternal.RunForOutput(
				"msbuild",
				arguments,
				out var exitCode,
				timeoutInSeconds: 60,
				output: _output);

			Assert.True(exitCode == 0, $"MSBuild property evaluation failed:{Environment.NewLine}{output}");
			Assert.True(File.Exists(resultFile), $"MSBuild result file does not exist: {resultFile}");

			using var result = JsonDocument.Parse(File.ReadAllText(resultFile));
			var properties = result.RootElement.GetProperty("Properties");

			return (
				GetProperty(properties, "_NormalizedMSBuildExtensionsPath"),
				GetProperty(properties, "_WorkloadManifestInstallDirectory"),
				GetProperty(properties, "_WorkloadInstallDotNetPath"),
				GetProperty(properties, "DotNetSdkManifestsFolder"));
		}
		finally
		{
			if (File.Exists(resultFile))
				File.Delete(resultFile);
		}
	}

	static string GetProperty(JsonElement properties, string propertyName)
	{
		var value = properties.TryGetProperty(propertyName, out var property) &&
			property.ValueKind == JsonValueKind.String
				? property.GetString()
				: null;

		if (string.IsNullOrEmpty(value))
			throw new XunitException($"MSBuild property '{propertyName}' was empty.");

		return value;
	}
}
