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
        // Wait for the WebView to load the initial content before clicking the link
        Thread.Sleep(1000);

        // Navigate to the bundled document without depending on an external site.
        App.Tap("ClickLinkButton");
        Assert.That(App.WaitForTextToBePresentInElement("CanGoBackLabel", "CanGoBack: True"), Is.True);
        
        App.Tap("UpdateStatusButton");
        Assert.That(App.WaitForTextToBePresentInElement("CanGoBackLabel", "CanGoBack: True"), Is.True);
        Assert.That(App.WaitForTextToBePresentInElement("CanGoForwardLabel", "CanGoForward: False"), Is.True);

        App.Tap("GoBackButton");

        App.Tap("UpdateStatusButton");
        Assert.That(App.WaitForTextToBePresentInElement("CanGoBackLabel", "CanGoBack: False"), Is.True);
        Assert.That(App.WaitForTextToBePresentInElement("CanGoForwardLabel", "CanGoForward: True"), Is.True);

        App.Tap("GoForwardButton");

        App.Tap("UpdateStatusButton");
        Assert.That(App.FindElement("CanGoBackLabel").GetText(), Is.EqualTo("CanGoBack: True"));
        Assert.That(App.FindElement("CanGoForwardLabel").GetText(), Is.EqualTo("CanGoForward: False"));
    }
}
#endif