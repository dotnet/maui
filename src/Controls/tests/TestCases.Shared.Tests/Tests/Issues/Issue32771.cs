using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

[Category(UITestCategories.CollectionView4)]
public class Issue32771 : _IssuesUITest
{
    public Issue32771(TestDevice testDevice) : base(testDevice)
    {
    }
    public override string Issue => "Flow direction not working on Header/Footer in CollectionView [iOS]";

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void FlowdirectionShouldWorkForHeaderFooter()
    {
        App.WaitForElement("ToggleFlowDirectionButton");
        App.Tap("ToggleFlowDirectionButton");
        VerifyScreenshot();
    }
}
