using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue27766 : _IssuesUITest
{
    public override string Issue => "The bottom content inset value does not need to be updated for CollectionView items when the editor is an inner element";

    public Issue27766(TestDevice device) : base(device)
    {
    }

    [Test]
    [Category(UITestCategories.CollectionView)]
    public void ShouldIgnoreBottomContentInsetForCollectionViewItems()
    {
        App.WaitForElement("Test 3");
        App.Tap("Test 3");
        Thread.Sleep(500); // Add some wait for poping up the keyboard to resolve flakiness in CI.

        // CI tests sometimes fail due to color inconsistency between the keyboard and bottom navigation bar.
#if ANDROID
        VerifyScreenshot(cropTop: 63, cropBottom: 126);
#else
        VerifyScreenshot();
#endif
    }
}
