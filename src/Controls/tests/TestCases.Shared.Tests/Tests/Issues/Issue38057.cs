using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue38057 : _IssuesUITest
{
    public Issue38057(TestDevice testDevice) : base(testDevice)
    {
    }

    public override string Issue => "CollectionView MakeVisible scrolls the target item to the top";

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void Issue38057_MakeVisiblePositionsTargetAtBottomOfViewport()
    {
        App.WaitForElement("ScrollToProboscisMonkeyButton");
        App.Tap("ScrollToProboscisMonkeyButton");
        VerifyScreenshot();
    }
}
