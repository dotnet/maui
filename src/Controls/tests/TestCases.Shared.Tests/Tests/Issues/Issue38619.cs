using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38619 : _IssuesUITest
{
	public override string Issue => "Window.Page is blank after restoring a page with a custom TitleBar";

	public Issue38619(TestDevice device)
		: base(device)
	{
	}

	[Test]
	[ShardedTestCategory(UITestCategories.Shell, shard: 1)]
	public void OriginalPageRendersAfterWindowPageIsRestored()
	{
		App.WaitForElement("OriginalPageRenderedLabel");
		App.Tap("ReplaceWindowPageButton");
		App.WaitForElement("ReplacementPageRenderedLabel");
		App.Tap("RestoreOriginalPageButton");

		var originalPageLabel = App.WaitForElement("OriginalPageRenderedLabel");

		Assert.That(originalPageLabel.GetText(), Is.EqualTo("Original page rendered"));
	}
}
