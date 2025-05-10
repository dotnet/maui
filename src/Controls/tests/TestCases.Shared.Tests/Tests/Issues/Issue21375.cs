#if !ANDROID
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;
using System.Text;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue21375 : _IssuesUITest
{
    public Issue21375(TestDevice device) : base(device) { }

    public override string Issue => "Selected CollectionView item is not announced";

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void SelectedItemsShowSelected ()
    {
        var collectionView = App.WaitForElement("collectionView");
        var centerX = collectionView.GetRect().X + (collectionView.GetRect().Width / 2);
        var firstItemY = collectionView.GetRect().Y + 25;
        var secondItemY = collectionView.GetRect().Y + 75;
        var thirdItemY = collectionView.GetRect().Y + 125;

        App.TapCoordinates(centerX, firstItemY);
        App.WaitForElement("calculateButton").Tap();

        VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_single");

        App.WaitForElement("multipleButton").Tap();
        App.TapCoordinates(centerX, secondItemY);
        App.TapCoordinates(centerX, thirdItemY);
        App.WaitForElement("calculateButton").Tap();

        VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_multiple");

        App.WaitForElement("noneButton").Tap();
        App.WaitForElement("calculateButton").Tap();
        VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_none");

        App.WaitForElement("singleButton").Tap();
        App.WaitForElement("calculateButton").Tap();

#if !WINDOWS // Windows clears out the selected items when we switch back
        VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_single");

        App.WaitForElement("multipleButton").Tap();
        App.WaitForElement("calculateButton").Tap();

        VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_multiple");
#endif

    }
}
#endif
