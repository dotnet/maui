#if TEST_FAILS_ON_WINDOWS
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31911 : _IssuesUITest
{
    public Issue31911(TestDevice device) : base(device)
    {
    }

    public override string Issue => "CollectionView Header Footer not removed when set to null on Android";

    [Test, Order(1)]
    [Category(UITestCategories.CollectionView)]
    public void HeaderShouldBeRemovedWhenSetToNull()
    {
        App.WaitForElement("ToggleHeaderButton");
        App.Tap("ToggleHeaderButton");
        VerifyScreenshot();
    }

    [Test, Order(2)]
    [Category(UITestCategories.CollectionView)]
    public void FooterShouldBeRemovedWhenSetToNull()
    {
        App.WaitForElement("ToggleFooterButton");
        App.Tap("ToggleFooterButton");
        VerifyScreenshot();
    }
}
#endif