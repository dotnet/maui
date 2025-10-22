using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue32030 : _IssuesUITest
{
    public Issue32030(TestDevice device) : base(device) { }

    public override string Issue => "Android - WebView in a grid expands beyond it's cell";
    [Test]
    [Category(UITestCategories.WebView)]
    public void VerifyWebViewStaysWithinGridCell()
    {
        App.WaitForElement("TopLabel");
        VerifyScreenshot();
    }
}