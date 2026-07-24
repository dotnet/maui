#if IOS || MACCATALYST      //This is an iOS-specific issue and can be reproduced by extending UIButton. Therefore, the test was added only for iOS and Mac Catalyst.
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue36749 : _IssuesUITest
{
    public Issue36749(TestDevice device) : base(device) { }

    public override string Issue => "ButtonHandler clears native platform-view styling set by custom handlers when Background/TextColor are null";

    [Test]
    [Category(UITestCategories.Button)]
    public void NativeStyledButtonShouldPreserveBackgroundColorOnInitialRender()
    {
        // This regression is iOS-only: the fix is in ButtonExtensions.UpdateBackground (iOS).
        if (Device != TestDevice.iOS)
            Assert.Ignore("Test is iOS-only: validates that UpdateBackground preserves native UIButton styling when Background is null.");

        App.WaitForElement("Issue36749Result");

        var resultText = App.FindElement("Issue36749Result").GetText();

        Assert.That(resultText, Is.EqualTo("PASS"),
            "Native BackgroundColor set in a UIButton subclass constructor should be preserved " +
            "when no cross-platform Background is assigned to the Button. " +
            "Regression: PR #33346 unconditionally reset BackgroundColor to UIColor.Clear " +
            "on the initial null-paint mapping, wiping native styling.");
    }
}
