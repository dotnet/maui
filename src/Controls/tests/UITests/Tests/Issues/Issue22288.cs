using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue22288 : _IssuesUITest
	{
		public Issue22288(TestDevice device) : base(device) { }

		public override string Issue => "Top Button Content Causes Infinite Layout";

		[Test]
		[Category(UITestCategories.Button)]
		public void AppDoesntFreezeWhenRotatingDevice()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Mac, TestDevice.Windows });
            try
            {
                App.SetOrientationPortrait();
			    var portraitRect = App.WaitForElement("outerScrollView").GetRect();
                App.SetOrientationLandscape();
			    var landscapeRect = App.WaitForElement("outerScrollView").GetRect();

                Assert.Greater(landscapeRect.Width, portraitRect.Width);

            }
            finally
            {
                App.SetOrientationPortrait();
            }
		}
	}
}