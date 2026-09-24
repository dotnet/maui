#if TEST_FAILS_ON_ANDROID && TEST_FAILS_ON_WINDOWS    //Using JavaScript to click the URL is not working on Android and Windows.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue30381 : _IssuesUITest
{
	public override string Issue => "WebView GoBack/GoForward not working for HtmlWebViewSource on iOS";

	public Issue30381(TestDevice device) : base(device)
	{
	}

	[Test]
	[Category(UITestCategories.WebView)]
	public void WebViewCanGoForwardShouldHaveValueAfterNavigation()
	{
		App.WaitForTextToBePresentInElement("PageTitleLabel", "Inline page");

		App.Tap("ClickLinkButton");
		App.WaitForTextToBePresentInElement("PageTitleLabel", "Linked page");
		App.WaitForTextToBePresentInElement("CanGoBackLabel", "CanGoBack: True");

		App.Tap("UpdateStatusButton");
		App.WaitForTextToBePresentInElement("CanGoBackLabel", "CanGoBack: True");
		App.WaitForTextToBePresentInElement("CanGoForwardLabel", "CanGoForward: False");

		App.Tap("GoBackButton");
		App.WaitForTextToBePresentInElement("PageTitleLabel", "Inline page");

		App.Tap("UpdateStatusButton");
		App.WaitForTextToBePresentInElement("CanGoBackLabel", "CanGoBack: False");
		App.WaitForTextToBePresentInElement("CanGoForwardLabel", "CanGoForward: True");

		App.Tap("GoForwardButton");
		App.WaitForTextToBePresentInElement("PageTitleLabel", "Linked page");

		App.Tap("UpdateStatusButton");
		Assert.That(App.FindElement("CanGoBackLabel").GetText(), Is.EqualTo("CanGoBack: True"));
		Assert.That(App.FindElement("CanGoForwardLabel").GetText(), Is.EqualTo("CanGoForward: False"));
	}
}
#endif