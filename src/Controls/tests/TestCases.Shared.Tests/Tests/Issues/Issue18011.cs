using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue18011 : _IssuesUITest
{
    public Issue18011(TestDevice device) : base(device) { }

    public override string Issue => "RadioButton TextColor for plain Content not working on iOS";

    [Test]
    [Category(UITestCategories.RadioButton)]
    public void VerifyRadioButtonTextColor()
    {
        App.WaitForElement("Issue18011_RadioButton");
        VerifyScreenshot();
    }
}