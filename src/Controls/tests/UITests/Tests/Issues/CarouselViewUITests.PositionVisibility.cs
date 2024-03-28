using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class CarouselViewPositionVisibility : _IssuesUITest
	{
		public CarouselViewPositionVisibility(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "[Bug] CarouselView position resets when visibility toggled";

		// Issue12848 (src\ControlGallery\src\Issues.Shared\Issue12848.cs
		[Test]
		public void Issue12848Test()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS, TestDevice.Mac, TestDevice.Windows },
				"Android specific Test");

			App.WaitForElement("TestCarouselView");
			App.SwipeRightToLeft("TestCarouselView");
			var position1 = App.FindElement("CarouselPosition").GetText();
			Assert.NotNull(position1);
			Assert.AreEqual(1, int.Parse(position1!));
			App.Click("HideButton");
			App.Click("ShowButton");
			var position2 = App.FindElement("CarouselPosition").GetText();
			Assert.NotNull(position2);
			Assert.AreEqual(1, int.Parse(position2!));
			App.Screenshot("Test passed");
		}
	}
}
