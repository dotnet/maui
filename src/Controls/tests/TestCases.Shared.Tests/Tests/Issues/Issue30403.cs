using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue30403 : _IssuesUITest
    {
        public Issue30403(TestDevice device)
            : base(device)
        { }

        public override string Issue => "Image under WinUI does not respect VerticalOptions and HorizontalOptions";

        [Test]
        [Category(UITestCategories.Layout)]
        [Category(UITestCategories.Image)]
        public void ImageShouldRespectLayoutOptions()
        {
            // Wait for images to load
            App.WaitForElement("CenteredImage");
            App.WaitForElement("StartImage");
            App.WaitForElement("EndImage");
            App.WaitForElement("FillImage");
            App.WaitForElement("WideImage");

            // On Windows, images should be positioned according to their alignment options
            // instead of stretching to fill the container
            VerifyScreenshot();
        }
    }
}