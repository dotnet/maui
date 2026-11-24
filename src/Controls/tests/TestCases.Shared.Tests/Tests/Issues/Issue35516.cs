using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue35516 : _IssuesUITest
{
	public Issue35516(TestDevice device) : base(device)
	{
	}

	public override string Issue => "SearchHandler Query should update dynamically";

	[Test]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void QueryAndTextTransformShouldUpdateSearchText()
	{
		App.WaitForElement("UpdateQueryButton");
		App.Tap("UpdateQueryButton");
		var searchText = App.GetShellSearchHandler().GetText();
		Assert.That(searchText, Is.EqualTo("HELLO WORLD"));
	}
}
