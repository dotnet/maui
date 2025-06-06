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

        App.Tap("Item 1");
        App.WaitForElement("calculateButton").Tap();

        VerifyScreenshot(TestContext.CurrentContext.Test.MethodName + "_single");

        App.WaitForElement("multipleButton").Tap();
        App.Tap("Item 2");
        App.Tap("Item 3");
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

