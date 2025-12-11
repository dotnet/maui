using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32971 : _IssuesUITest
{
	public override string Issue => "WebView content does not scroll when placed inside a ScrollView";

	public Issue32971(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebViewShouldScrollInsideScrollView()
	{
		App.WaitForElement("TestScrollView");
		App.ScrollDown("TestScrollView");
		App.Tap("CheckButton");
		var scrollStateLabel = App.FindElement("ScrollStateLabel").GetText();
		Assert.That(scrollStateLabel, Is.EqualTo("Scrolled"));
	}
}