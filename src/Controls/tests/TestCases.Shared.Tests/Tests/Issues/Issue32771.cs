using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32771 : _IssuesUITest
{
    public Issue32771(TestDevice testDevice) : base(testDevice)
    {
    }
    public override string Issue => "Flow direction not working on Header/Footer in CollectionView [iOS]";

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void ShellTitleShouldNotDisappear()
    {
        App.WaitForElement("ToggleFlowDirectionButton");
        App.Tap("ToggleFlowDirectionButton");
        VerifyScreenshot();
    }
}