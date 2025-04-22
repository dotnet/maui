using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues;
public class Issue7432 : _IssuesUITest
{
    public Issue7432(TestDevice device) : base(device) { }

    public override string Issue => "Android Image.Scale produces wrong layout";

    [Test]
    [Category(UITestCategories.Image)]
    public void ImageShouldScaleProperly()
    {
        App.WaitForElement("Image");
        VerifyScreenshot();
    }
}