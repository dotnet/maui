using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue12848 : IssuesUITest
	{
		public Issue12848(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] CarouselView position resets when visibility toggled";

		[Test]
		[Category(UITestCategories.CarouselView)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void Issue12848Test()
		{
			RunningApp.WaitForElement("TestCarouselView");
			RunningApp.SwipeRightToLeft();
			int.TryParse(RunningApp.FindElement("CarouselPosition").GetText(), out int position1);
			ClassicAssert.AreEqual(1, position1);
			RunningApp.Tap("HideButton");
			RunningApp.Tap("ShowButton");
			int.TryParse(RunningApp.FindElement("CarouselPosition").GetText(), out int position2);
			ClassicAssert.AreEqual(1, position2);
			RunningApp.Screenshot("Test passed");
		}
	}
}