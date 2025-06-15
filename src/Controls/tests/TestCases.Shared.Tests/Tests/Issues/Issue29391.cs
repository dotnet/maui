#if IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue29391 : _IssuesUITest
	{
		public Issue29391(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "[iOS] IsSwipeEnabled Not Working on CarouselView (CV2)";

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void IsSwipeEnabledDisablesSwipeGestures()
		{
			// Verify initial state: IsSwipeEnabled = true (Switch is toggled on)
			App.WaitForElement("Item1");

			// Verify swiping works when enabled
			App.ScrollRight("CarouselView", swipePercentage: 0.8);
			App.WaitForElement("Item2");

			// Disable swipe gestures
			App.Tap("Switch");

			// Verify swiping is disabled - should stay on Item2
			App.ScrollRight("CarouselView", swipePercentage: 0.8);
			App.WaitForElement("Item2");

			// Verify we cannot swipe back either
			App.ScrollLeft("CarouselView", swipePercentage: 0.8);
			App.WaitForElement("Item2");
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void IsSwipeEnabledCanBeToggledBack()
		{
			// Start with swipe enabled
			App.WaitForElement("Item1");
			App.ScrollRight("CarouselView", swipePercentage: 0.8);
			App.WaitForElement("Item2");

			// Disable swiping
			App.Tap("Switch");
			App.ScrollRight("CarouselView", swipePercentage: 0.8);
			App.WaitForElement("Item2");

			// Re-enable swiping
			App.Tap("Switch");

			// Verify swiping works again
			App.ScrollRight("CarouselView", swipePercentage: 0.8);
			App.WaitForElement("Item3");
		}
	}
}
#endif