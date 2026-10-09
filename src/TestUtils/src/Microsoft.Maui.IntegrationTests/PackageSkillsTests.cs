using System.IO.Compression;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class PackageSkillsTests : BaseBuildTest
{
	public PackageSkillsTests(IntegrationTestFixture fixture, ITestOutputHelper output)
		: base(fixture, output)
	{
	}

	[Fact]
	public void ControlsPackageContainsVersionedSkillsAndReferences()
	{
		var mauiDirectory = TestEnvironment.GetMauiDirectory();
		var packageOverride = Environment.GetEnvironmentVariable("MAUI_CONTROLS_TEST_PACKAGE");
		var packagePath = string.IsNullOrWhiteSpace(packageOverride)
			? Path.Combine(mauiDirectory, "artifacts", $"Microsoft.Maui.Controls.{MauiPackageVersion}.nupkg")
			: Path.GetFullPath(packageOverride);
		Assert.True(File.Exists(packagePath), $"Controls package '{packagePath}' does not exist.");

		using var archive = ZipFile.OpenRead(packagePath);
		var nuspecEntry = Assert.Single(archive.Entries, entry => entry.FullName.EndsWith(".nuspec", StringComparison.Ordinal));
		using (var stream = nuspecEntry.Open())
		{
			var nuspec = XDocument.Load(stream);
			var ns = nuspec.Root!.Name.Namespace;
			Assert.Equal("Microsoft.Maui.Controls", nuspec.Root.Element(ns + "metadata")!.Element(ns + "id")!.Value);
			Assert.Equal(MauiPackageVersion, nuspec.Root.Element(ns + "metadata")!.Element(ns + "version")!.Value);
		}

		var sourceRoot = Path.Combine(mauiDirectory, "src", "Controls", "src", "NuGet");
		var skillRoot = Path.Combine(sourceRoot, "skills");
		Assert.True(Directory.Exists(skillRoot), $"Skill source directory '{skillRoot}' does not exist.");
		var expectedFiles = Directory.GetFiles(skillRoot, "*.md", SearchOption.AllDirectories)
			.Select(path => Path.GetRelativePath(sourceRoot, path).Replace('\\', '/'))
			.OrderBy(path => path, StringComparer.Ordinal)
			.ToArray();
		Assert.NotEmpty(expectedFiles);
		var skillEntries = archive.Entries.Where(entry => entry.FullName.StartsWith("skills/", StringComparison.Ordinal)).ToArray();
		Assert.Equal(expectedFiles, skillEntries.Select(entry => entry.FullName).OrderBy(path => path, StringComparer.Ordinal));
		foreach (var entry in skillEntries)
		{
			using var stream = entry.Open();
			using var contents = new MemoryStream();
			stream.CopyTo(contents);
			Assert.Equal(File.ReadAllBytes(Path.Combine(sourceRoot, entry.FullName)), contents.ToArray());
		}

		var skillHeaders = skillEntries.Where(entry => entry.FullName.EndsWith("/SKILL.md", StringComparison.Ordinal)).ToArray();
		Assert.NotEmpty(skillHeaders);
		Assert.Contains(skillHeaders, entry => entry.FullName == "skills/microsoft-maui-controls-upgrade-to-11/SKILL.md");
		foreach (var header in skillHeaders)
		{
			var segments = header.FullName.Split('/');
			Assert.Equal(3, segments.Length);
			using var reader = new StreamReader(header.Open());
			var text = reader.ReadToEnd().Replace("\r\n", "\n", StringComparison.Ordinal);
			Assert.StartsWith("---\n", text, StringComparison.Ordinal);
			var frontmatterEnd = text.IndexOf("\n---\n", 4, StringComparison.Ordinal);
			Assert.True(frontmatterEnd > 0, $"Missing closing frontmatter in '{header.FullName}'.");
			var frontmatter = text[..frontmatterEnd];
			Assert.Matches($"(?m)^name: {Regex.Escape(segments[1])}$", frontmatter);
			Assert.Matches("(?m)^description: .+", frontmatter);
			foreach (Match link in Regex.Matches(text, @"\[[^\]]*\]\((references/[^)\s]+\.md)\)"))
			{
				var reference = $"skills/{segments[1]}/{link.Groups[1].Value}";
				Assert.Contains(skillEntries, entry => entry.FullName == reference);
			}
		}
	}
}
