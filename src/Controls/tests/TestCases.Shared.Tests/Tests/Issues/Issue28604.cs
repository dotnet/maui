using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue28604 : _IssuesUITest
{
    public Issue28604(TestDevice testDevice) : base(testDevice)
    {
    }
    public override string Issue => "Footer Not Displayed at the Bottom When EmptyView is Active in CV2";

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void FooterShouldDisplayAtBottomOfEmptyView()
    {
        App.WaitForElement("CollectionView");
        VerifyScreenshot();
    }
}