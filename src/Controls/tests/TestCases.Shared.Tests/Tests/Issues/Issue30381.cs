# if TEST_FAILS_ON_ANDROID      //Using JavaScript to click the URL is not working on Android.
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
        Thread.Sleep(2000);

        // Click the link via JavaScript to navigate to external URL
        App.Tap("ClickLinkButton");

        // Wait for navigation to complete
        // Add a delay to allow navigation to process
        Thread.Sleep(2000);
        App.Tap("UpdateStatusButton");
        Assert.That(App.FindElement("CanGoBackLabel").GetText(), Is.EqualTo("CanGoBack: True"));
        Assert.That(App.FindElement("CanGoForwardLabel").GetText(), Is.EqualTo("CanGoForward: False"));

        App.Tap("GoBackButton");

        App.Tap("UpdateStatusButton");
        Assert.That(App.FindElement("CanGoBackLabel").GetText(), Is.EqualTo("CanGoBack: False"));
        Assert.That(App.FindElement("CanGoForwardLabel").GetText(), Is.EqualTo("CanGoForward: True"));

        App.Tap("GoForwardButton");

        App.Tap("UpdateStatusButton");
        Assert.That(App.FindElement("CanGoBackLabel").GetText(), Is.EqualTo("CanGoBack: True"));
        Assert.That(App.FindElement("CanGoForwardLabel").GetText(), Is.EqualTo("CanGoForward: False"));
    }
}
#endif