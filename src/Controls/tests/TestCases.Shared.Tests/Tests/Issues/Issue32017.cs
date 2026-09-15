#if WINDOWS // This issue is specific to Windows window resizing behavior
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;

public class Issue32017 : _IssuesUITest
{
    public Issue32017(TestDevice device) : base(device) { }

    public override string Issue => "Image shifts downward when window is resized smaller";

    [Test]
    [Category(UITestCategories.CarouselView)]
    public void CarouselViewImageDoesNotShiftAfterWindowResize()
    {
        // Wait for CarouselView to load
        App.WaitForElement("TestCarouselView");

        // Reduce the window width
        App.Tap("ReduceWidthButton");

        // Restore the window to original size
        App.Tap("RestoreWidthButton");

        // Verify the image position is correct after resize cycle
        VerifyScreenshot();
    }
}
#endif