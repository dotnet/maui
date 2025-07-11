using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue15508 : _IssuesUITest
{
	public Issue15508(TestDevice device) : base(device) { }

	public override string Issue => "Scrollview.ScrollTo execution only returns after manual scroll";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollViewinAwait()
	{
		App.WaitForElement("Button");
		App.Tap("Button");
		var isTextPresent = App.WaitForTextToBePresentInElement("Label", "Scroll Completed");
		Assert.That(isTextPresent, Is.True, "The text 'Scroll Completed' was not found in the element.");
	}
}

