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
		var scrollLabel = App.FindElement("Label");
		Assert.That(scrollLabel.GetText(), Is.EqualTo("Scroll Completed"));
	}
}

