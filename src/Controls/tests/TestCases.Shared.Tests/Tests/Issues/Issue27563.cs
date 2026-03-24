using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27563 : _IssuesUITest
	{
		public override string Issue => "[Windows] CarouselView Scrolling Issue";

		public Issue27563(TestDevice device)
		: base(device)
		{ }

#if !WINDOWS // On Windows, TimeoutException is thrown when enabling the Loop. Refer issue: https://github.com/dotnet/maui/issues/29245
		[Test, Order(1)]
		[Category(UITestCategories.CarouselView)]
		public void VerifyCarouselViewIndicatorPositionWithoutLooping()
		{
			App.WaitForElement("carouselview");
			App.WaitForElement("Remain View");

			App.Tap("ScrollToSecondButton");
			App.WaitForElement("Actual View");

			App.Tap("PositionButton");
			App.WaitForElement("Percentage View");

			App.Tap("PingButton");
			App.WaitForElement("Ping:1");
			App.Tap("ScrollToSecondButton");
		}
#endif

#if !WINDOWS && !MACCATALYST
		// On Catalyst, Swipe actions not supported in Appium.
		// On Windows, TimeoutException is thrown when enabling the Loop. Refer issue: https://github.com/dotnet/maui/issues/29245
		[Test, Order(2)]
		[Category(UITestCategories.CarouselView)]
		public void VerifyCarouselViewScrolling()
		{
			App.WaitForElement("carouselview");
			App.SwipeRightToLeft("carouselview");
			App.WaitForElement("Percentage View");
			App.Tap("PositionButton");
			VerifyScreenshot();
		}
#endif
	}
}
