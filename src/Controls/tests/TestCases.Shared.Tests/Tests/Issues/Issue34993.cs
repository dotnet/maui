using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue34993 : _IssuesUITest
{
    public Issue34993(TestDevice testDevice) : base(testDevice) { }

    public override string Issue => "RadioButton still shows gradient after background changed to solid color brush";

    [Test]
    [Category(UITestCategories.RadioButton)]
    public void VerifyRadioButtonBackgroundUpdatesFromGradientToSolidColor()
    {
        Exception? exception = null;

        App.WaitForElement("BackgroundRadioButton");
        VerifyScreenshotOrSetException(ref exception, "GradientBackgroundRadioButton");
        App.WaitForElement("ChangeBackgroundButton");
        App.Tap("ChangeBackgroundButton");
        VerifyScreenshotOrSetException(ref exception, retryTimeout: TimeSpan.FromSeconds(2));

        if (exception is not null)
        {
            throw exception;
        }
    }
}
