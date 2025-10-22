using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32042 : _IssuesUITest
{
    public Issue32042(TestDevice testDevice) : base(testDevice)
    {
    }
    public override string Issue => "Rectangle appears blurred on iOS and macOS when its bounds are changed at runtime within an AbsoluteLayout";

    [Test]
    [Category(UITestCategories.Shape)]
    public void RectangleShouldNotBeBlurredAfterBoundsChange()
    {
        App.WaitForElement("Issue32042Button");
        App.Tap("Issue32042Button");
        VerifyScreenshot();
    }
}