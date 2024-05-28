using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
    public class Issue21314 : _IssuesUITest {
        public override string Issue => "Image has wrong orientation on iOS";

        public Issue21314(TestDevice device) : base(device)
        {
        }

        [Test]
        public void ImageShouldBePortrait()
        {
            this.IgnoreIfPlatforms (new TestDevice[] { TestDevice.Android, TestDevice.Windows });

            var image = App.WaitForElement ("theImage").GetRect();
			ClassicAssert.Greater(image.Height, image.Width);
        }
    }
}