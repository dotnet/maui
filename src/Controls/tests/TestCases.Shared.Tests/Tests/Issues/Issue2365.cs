using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue2365 : _IssuesUITest
{
    public Issue2365(TestDevice testDevice) : base(testDevice)
    {
    }
    public override string Issue => "[Enhancement] BoxView handlers should use Paint";

    [Test]
    [Category(UITestCategories.BoxView)]
    public void BoxViewColorShouldRenderCorrectly()
    {
        App.WaitForElement("SetFillNullButton");
        App.Tap("SetFillNullButton");
        VerifyScreenshot();
    }
}