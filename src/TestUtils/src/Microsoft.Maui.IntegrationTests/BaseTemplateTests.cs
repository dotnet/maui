using System.Xml.Linq;

namespace Microsoft.Maui.IntegrationTests;

public abstract class BaseTemplateTests : BaseBuildTest
{
	protected const string AvaloniaBuildSkipReason = "Avalonia packages are not available on dotnet-public. See https://github.com/dotnet/maui/pull/35950";

	protected BaseTemplateTests(IntegrationTestFixture fixture, ITestOutputHelper output) : base(fixture, output)
	{
		// Constructor setup (equivalent to [SetUp])
		File.Copy(Path.Combine(TestEnvironment.GetMauiDirectory(), "src", "Templates", "tests", "Directory.Build.props"),
			Path.Combine(TestDirectory, "Directory.Build.props"), true);
		File.Copy(Path.Combine(TestEnvironment.GetMauiDirectory(), "src", "Templates", "tests", "Directory.Build.targets"),
			Path.Combine(TestDirectory, "Directory.Build.targets"), true);
	}

	protected void AssertContains(string expected, string actual)
	{
		Assert.True(
			actual.Contains(expected, StringComparison.Ordinal),
			$"Expected string '{actual}' to contain '{expected}'.");
	}

	protected void AssertDoesNotContain(string expected, string actual)
	{
		Assert.False(
			actual.Contains(expected, StringComparison.Ordinal),
			$"Expected string '{actual}' to not contain '{expected}'.");
	}

	/// <summary>
	/// Writes a NuGet.config that maps the Avalonia packages to nuget.org, since they are not
	/// available on the feeds the other template packages restore from.
	/// </summary>
	protected string CreateAvaloniaNuGetConfig(string projectDir)
	{
		var config = XDocument.Load(TestNuGetConfig);
		var packageSources = config.Root!.Element("packageSources")!;
		const string nugetOrg = "nuget.org";

		packageSources.Add(
			new XElement("add",
				new XAttribute("key", nugetOrg),
				new XAttribute("value", "https://api.nuget.org/v3/index.json"),
				new XAttribute("protocolVersion", "3")));

		var sourceMapping = new XElement("packageSourceMapping");
		foreach (var source in packageSources.Elements("add"))
		{
			var key = source.Attribute("key")!.Value;
			var patterns = key == nugetOrg ? new[] { "Avalonia*", "MicroCom.*" } : new[] { "*" };
			sourceMapping.Add(
				new XElement("packageSource",
					new XAttribute("key", key),
					patterns.Select(pattern =>
						new XElement("package",
							new XAttribute("pattern", pattern)))));
		}

		config.Root.Add(sourceMapping);
		var path = Path.Combine(projectDir, "NuGet.config");
		config.Save(path);
		return path;
	}

	protected void AssertIncludesRootGitIgnore(string projectDir)
	{
		var gitIgnorePath = Path.Combine(projectDir, ".gitignore");

		Assert.True(File.Exists(gitIgnorePath),
			$"Expected '{gitIgnorePath}' to exist.");

		var gitIgnoreContents = File.ReadAllText(gitIgnorePath);
		AssertContains("bin/", gitIgnoreContents);
		AssertContains("obj/", gitIgnoreContents);
	}
}
