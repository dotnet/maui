using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue15508 : _IssuesUITest
{
	public Issue15508(TestDevice device) : base(device) { }

	public override string Issue => "Scrollview.ScrollTo execution only returns after manual scroll";
	const string ButtonToScroll = "ButtonToScroll";

	[Test]
	[Category(UITestCategories.ScrollView)]
	public void ScrollViewinAwait()
	{
		App.WaitForElement(ButtonToScroll);
		App.Tap(ButtonToScroll);
		var scrollLabel = App.FindElement("ScrollLabel");
		Assert.That(scrollLabel.GetText(), Is.EqualTo("Scroll Completed"));
	}
}

