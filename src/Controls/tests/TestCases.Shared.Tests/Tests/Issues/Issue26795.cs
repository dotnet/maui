using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue26795 : _IssuesUITest
{
    public Issue26795(TestDevice device) : base(device) { }

    public override string Issue => "Specifying HeightRequest in Webview when wrapped by ScrollView set invisible causes crash in iOS";

    [Test]
    [Category(UITestCategories.WebView)]
    public void WebViewShouldNotCrash()
    {
        App.WaitForElement("Label");
    }
}