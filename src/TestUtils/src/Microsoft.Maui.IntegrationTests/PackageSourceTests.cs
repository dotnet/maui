using System.Xml.Linq;

namespace Microsoft.Maui.IntegrationTests;

[Trait("Category", "Build")]
public class PackageSourceTests
{
	[Fact]
	public void IdenticalSourcesAreDeduplicatedWithoutChangingOtherEntries()
	{
		var sources = XElement.Parse("""
			<packageSources>
			  <clear />
			  <add key="feed" value="https://example.com/v3/index.json" />
			  <add key="feed" value="https://example.com/v3/index.json" />
			  <add key="other" value="https://example.org/v3/index.json" />
			</packageSources>
			""");

		BaseTemplateTests.RemoveDuplicatePackageSources(sources);

		Assert.Equal(new[] { "clear", "add", "add" }, sources.Elements().Select(element => element.Name.LocalName));
		Assert.Equal(new[] { "feed", "other" }, sources.Elements("add").Select(element => element.Attribute("key")!.Value));
	}

	[Theory]
	[InlineData("feed", "https://example.org/v3/index.json")]
	[InlineData("FEED", "https://example.com/v3/index.json")]
	public void ConflictingSourceDefinitionsAreRejected(string key, string value)
	{
		var sources = new XElement("packageSources",
			new XElement("add", new XAttribute("key", "feed"), new XAttribute("value", "https://example.com/v3/index.json")),
			new XElement("add", new XAttribute("key", key), new XAttribute("value", value)));

		var error = Assert.Throws<InvalidOperationException>(() => BaseTemplateTests.RemoveDuplicatePackageSources(sources));

		Assert.Contains("conflicting definitions", error.Message, StringComparison.Ordinal);
		Assert.Equal(2, sources.Elements("add").Count());
	}
}
