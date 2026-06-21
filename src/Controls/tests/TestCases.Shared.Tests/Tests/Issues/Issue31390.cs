#if TEST_FAILS_ON_ANDROID // https://github.com/dotnet/maui/issues/22116
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue31390 : _IssuesUITest
{
    public Issue31390(TestDevice testDevice) : base(testDevice)
    {
    }
    public override string Issue => "System.ArgumentException thrown when setting FlyoutLayoutBehavior dynamically";

    [Test]
    [Category(UITestCategories.FlyoutPage)]
    public void EnsureFlyoutLayoutBehaviorChanges()
    {
        App.WaitForElement("GoToNextPage");
        App.Tap("GoToNextPage");
        App.Tap("ChangeToPopover");
        App.Tap("Apply");
        App.TapFlyoutPageIcon("Flyout");
        App.WaitForElement("Issue31390_FlyoutLabel");
    }
}
#endif