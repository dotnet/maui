using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue36302 : _IssuesUITest
{
    public Issue36302(TestDevice testDevice) : base(testDevice) { }

    public override string Issue => "Image and ImageButton backgrounds not cleared on iOS/MacCatalyst when set to null";

    [Test]
    [Category(UITestCategories.Image)]
    public void ImageAndImageButtonBackgroundClearsWhenSetToNull()
    {
        App.WaitForElement("ClearBackgroundsButton");
        App.Tap("ClearBackgroundsButton");
        VerifyScreenshot();
    }
}
