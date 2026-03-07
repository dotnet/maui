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

		[SetUp]
		public void SetUp()
		{
			// Reset carousel to Item1 and ensure Switch is toggled on (IsSwipeEnabled=true)
			// so each test starts from a clean state regardless of prior test ordering.
			App.WaitForElement("CarouselView");

			// Scroll left repeatedly to get back to Item1
			App.ScrollLeft("CarouselView", swipePercentage: 0.8);
			App.ScrollLeft("CarouselView", swipePercentage: 0.8);
			App.ScrollLeft("CarouselView", swipePercentage: 0.8);

			App.WaitForElement("Item1");
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void IsSwipeEnabledDisablesSwipeGestures()
		{
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

			// Re-enable swiping so SetUp can scroll back for next test
			App.Tap("Switch");
		}

		[Test]
		[Category(UITestCategories.CarouselView)]
		public void IsSwipeEnabledCanBeToggledBack()
		{
			// Start with swipe enabled
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
