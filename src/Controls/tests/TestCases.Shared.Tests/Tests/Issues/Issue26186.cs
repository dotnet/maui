using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue26186(TestDevice testDevice) : _IssuesUITest(testDevice)
{
    public override string Issue => "When dynamically changing the CollectionView's width, the end portion of the CollectionView header's and footer's children is cut off";

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void ShouldHeaderFooterUpdateWithoutCroppingOnResizing()
    {
        App.WaitForElement("Button");
        App.Click("Button");
        VerifyScreenshot();
    }
}
