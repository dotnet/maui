#if ANDROID
using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
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
		[Category(UITestCategories.CarouselView)]
		public void Issue12848Test()
		{
			App.WaitForElement("TestCarouselView");
			App.SwipeRightToLeft("TestCarouselView");
			var position1 = App.FindElement("CarouselPosition").GetText();
			ClassicAssert.NotNull(position1);
			ClassicAssert.AreEqual(1, int.Parse(position1!));
			App.Click("HideButton");
			App.Click("ShowButton");
			var position2 = App.FindElement("CarouselPosition").GetText();
			ClassicAssert.NotNull(position2);
			ClassicAssert.AreEqual(1, int.Parse(position2!));
			App.Screenshot("Test passed");
		}
	}
}
#endif