using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34993 : _IssuesUITest
{
    public Issue34993(TestDevice testDevice) : base(testDevice) { }

    public override string Issue => "RadioButton background remains visible after being set to null";

    [Test]
    [Category(UITestCategories.RadioButton)]
    public void RadioButtonBackgroundClearsWhenSetToNull()
    {
        App.WaitForElement("BackgroundRadioButton");
        App.Tap("ResetBackgroundButton");
        VerifyScreenshot();
    }
}
