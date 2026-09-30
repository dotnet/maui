#if IOS || MACCATALYST || ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue36267 : _IssuesUITest
	{
		public Issue36267(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] IsSwipeEnabled Not Working on CarouselView (CV2) when set to false at load time";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void IsSwipeEnabledFalseFromStartDisablesSwipeGestures()
		{
			// IsSwipeEnabled is set to false from the moment the CarouselView is created
			// (not toggled afterward), matching the reproduction steps in issue 36267.
			App.WaitForElement("CarouselView");
			App.WaitForElement("Item1");

			// Attempting to swipe should have no effect while IsSwipeEnabled is false.
			App.ScrollRight("CarouselView", swipePercentage: 0.8);
			App.WaitForElement("Item1");

			var positionText = App.FindElement("PositionLabel").GetText();
			Assert.That(positionText, Is.EqualTo("Position: 0"));
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void IsSwipeEnabledCanBeEnabledAfterStartingDisabled()
		{
			App.WaitForElement("CarouselView");
			App.WaitForElement("Item1");

			// Confirm swipe is disabled initially.
			App.ScrollRight("CarouselView", swipePercentage: 0.8);
			App.WaitForElement("Item1");

			// Enable swipe gestures and verify swiping now works.
			App.Tap("ToggleSwipeButton");
			App.ScrollRight("CarouselView", swipePercentage: 0.8);
			App.WaitForElement("Item2");

			// Swipe back and disable swipe again to restore state for other tests
			// (tests share the same app instance and may run in any order).
			App.ScrollLeft("CarouselView", swipePercentage: 0.8);
			App.WaitForElement("Item1");
			App.Tap("ToggleSwipeButton");
		}
	}
}
#endif
