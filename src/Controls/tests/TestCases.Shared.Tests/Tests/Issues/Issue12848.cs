using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue12848 : _IssuesUITest
	{
		public Issue12848(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Bug] CarouselView position resets when visibility toggled";

		[Test]
		[Category(UITestCategories.CarouselView)]
		[Category(UITestCategories.Compatibility)]
		[FailsOnAndroid]
		[FailsOnIOS]
		public void Issue12848Test()
		{
			App.WaitForElement("TestCarouselView");
			App.SwipeRightToLeft();
			int.TryParse(App.FindElement("CarouselPosition").GetText(), out int position1);
			ClassicAssert.AreEqual(1, position1);
			App.Tap("HideButton");
			App.Tap("ShowButton");
			int.TryParse(App.FindElement("CarouselPosition").GetText(), out int position2);
			ClassicAssert.AreEqual(1, position2);
			App.Screenshot("Test passed");
		}
	}
}