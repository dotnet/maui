using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18011 : _IssuesUITest
{
    public override string Issue => "RadioButton TextColor for plain Content not working on iOS when Label styles present";

    public Issue18011(TestDevice device) : base(device) { }

    [Test]
    [Category(UITestCategories.RadioButton)]
    public void RadioButtonTextColorShouldNotBeOverriddenByGlobalLabelStyle()
    {
        App.WaitForElement("RadioButtonWithTextColor");
        VerifyScreenshot();
    }
}
