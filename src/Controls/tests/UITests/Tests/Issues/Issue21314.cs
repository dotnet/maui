using Maui.Controls.Sample;
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
    public class Issue21314 : _IssuesUITest {
        public override string Issue => "Image has wrong orientation on iOS";

        public Issue21314(TestDevice device) : base(device)
        {
        }

        [Test]
        public void ImageShouldBePortrait()
        {
            this.IgnoreIfPlatforms (new TestDevice[] { TestDevice.Mac, TestDevice.Android, TestDevice.Windows });

            _ = App.WaitForElement ("WaitForStubControl");
            VerifyScreenshot();
        }
    }
}