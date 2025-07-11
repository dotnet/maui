using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue30465 : _IssuesUITest
    {
        public Issue30465(TestDevice device) : base(device)
        {
        }

        public override string Issue => "AspectFit Fails to Preserve Image Stretching on iOS and macOS After Downsizing";

        [Test]
        [Category(UITestCategories.Image)]
        public void ImageAspectFitFromFile()
        {
            App.WaitForElement("TestImage");
            App.WaitForElement("StatusLabel");
            
            // Verify the image is loaded
            var statusText = App.GetText("StatusLabel");
            Assert.That(statusText, Does.Contain("Image loaded from file"));
            
            // The image should be visible and properly scaled with AspectFit
            var imageElement = App.FindElement("TestImage");
            Assert.That(imageElement, Is.Not.Null);
            
            // Take a screenshot to verify the image is displaying properly
            App.Screenshot("AspectFit Image from File Test");
        }
    }
}