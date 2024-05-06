using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue22000 : _IssuesUITest
	{
		public Issue22000(TestDevice device) : base(device) { }

		public override string Issue => "[Windows] Carousel changes the current position when window is resized";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void ResizeCarouselViewKeepsIndex()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Android, TestDevice.iOS, TestDevice.Mac });

			App.WaitForElement("WaitForStubControl");

			for (int i = 0; i < 10; i++)
			{
				App.Click("UpdateSizeButton");
			}

			VerifyScreenshot();
		}
	}
}