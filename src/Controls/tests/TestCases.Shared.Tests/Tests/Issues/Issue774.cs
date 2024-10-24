#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue774 : _IssuesUITest
{
    public Issue774(TestDevice device) : base(device)
    {
    }

    public override string Issue => "ActionSheet won't dismiss after rotation to landscape";

    [TearDown]
    protected void Teardown()
    {
        App.SetOrientationPortrait();
        App.Back();
    }

    [Test]
    [Category(UITestCategories.ActionSheet)]
    public void Issue774TestsDismissActionSheetAfterRotation()
    {
        App.WaitForElement("ShowActionSheet");
        App.Click("ShowActionSheet");
        App.Screenshot("Show ActionSheet");

        App.SetOrientationLandscape();
        App.Screenshot("Rotate Device");

        // Wait for the action sheet element to show up
        App.WaitForNoElement("What's up");

        var dismiss = App.FindElement("Dismiss");

        var target = dismiss != null ? "Dismiss" : "Destroy";

        App.Back();
        App.WaitForNoElement(target);

        App.Screenshot("Dismiss ActionSheet");
    }
}
#endif