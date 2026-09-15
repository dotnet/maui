using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38452 : _IssuesUITest
{
	public Issue38452(TestDevice device) : base(device)
	{
	}

	public override string Issue => "WebView inside a ScrollView blocks page scrolling when WebView has nothing to scroll";

	[Test]
	[Category(UITestCategories.WebView)]
	public void ParentScrollViewScrollsWhenDraggingOnNonScrollableWebView()
	{
		var initialScrollState = App.WaitForElement("ScrollStateLabel").GetText();
		Assert.That(initialScrollState, Is.EqualTo("NotScrolled"));

		var webViewRect = App.WaitForElement("WebViewContainer").GetRect();
		App.DragCoordinates(
			webViewRect.CenterX(),
			webViewRect.Y + webViewRect.Height - 20,
			webViewRect.CenterX(),
			webViewRect.Y + 20);

		var scrollState = App.WaitForElement("ScrollStateLabel").GetText();
		Assert.That(scrollState, Is.EqualTo("Scrolled"));
	}
}
