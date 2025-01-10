using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue26936(TestDevice testDevice) : _IssuesUITest(testDevice)
{
    public override string Issue => "CollectionView items appear with ellipses or dots when dynamically adding more items.";

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void ShouldCollectionViewEachItemsAppearWithoutDots()
    {
        App.WaitForElement("Label");
        for (int i = 0; i < 50; i++)
        {
            App.Tap("Button");
        }

        VerifyScreenshot();
    }
}
