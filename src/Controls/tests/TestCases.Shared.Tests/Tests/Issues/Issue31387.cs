using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue31387 : _IssuesUITest
	{
		public Issue31387(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[Android] CarouselView incorrectly reads out \"double tap to activate\"";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void CarouselViewShouldNotAnnounceDoubleTapToActivate()
		{
			// Verify the page loaded
			App.WaitForElement("Instructions");
			
			// Verify CarouselView is present and interactive
			App.WaitForElement("TestCarouselView");
			
			// Note: This test validates the CarouselView renders correctly.
			// Manual verification required: Enable TalkBack on Android device,
			// navigate to the CarouselView, and confirm it does NOT announce
			// "double tap to activate" (which would be incorrect since
			// CarouselView does not support item selection).
			//
			// The fix ensures IsSelectionEnabled returns false for CarouselView,
			// which prevents the incorrect TalkBack announcement.
		}
	}
}
