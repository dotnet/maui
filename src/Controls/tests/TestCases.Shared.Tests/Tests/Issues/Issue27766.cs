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

        // CI tests sometimes fail due to keyboard layout mismatching, As this case ensure the editor content is being visible, so keyboard is not needed for ensuring this case.
#if ANDROID || IOS
        VerifyScreenshot(cropBottom: 1200);
#else
        VerifyScreenshot();
#endif
    }
}
