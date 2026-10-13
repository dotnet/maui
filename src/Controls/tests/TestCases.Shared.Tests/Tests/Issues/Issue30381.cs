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
        App.WaitForElement("ClickLinkButton");
        AssertNavigationState("initial", canGoBack: false, canGoForward: false);

        App.Tap("ClickLinkButton");
        AssertNavigationState("target", canGoBack: true, canGoForward: false);

        App.Tap("GoBackButton");
        AssertNavigationState("initial", canGoBack: false, canGoForward: true);

        App.Tap("GoForwardButton");
        AssertNavigationState("target", canGoBack: true, canGoForward: false);
    }

    void AssertNavigationState(string document, bool canGoBack, bool canGoForward)
    {
        App.RetryAssert(() =>
        {
            App.Tap("UpdateStatusButton");
            Assert.That(App.WaitForElement("DocumentStatusLabel").GetText(), Is.EqualTo($"Document: {document}"));
            Assert.That(App.WaitForElement("CanGoBackLabel").GetText(), Is.EqualTo($"CanGoBack: {canGoBack}"));
            Assert.That(App.WaitForElement("CanGoForwardLabel").GetText(), Is.EqualTo($"CanGoForward: {canGoForward}"));
        });
    }
}
#endif