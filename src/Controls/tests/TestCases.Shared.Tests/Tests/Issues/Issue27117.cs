using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue27117 : _IssuesUITest
	{
		public Issue27117(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "CollectionView ScrollTo not working under android";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void ScrollToIndexZeroShowsFirstItemNotHeader()
		{
			// Wait for CollectionView to load
			App.WaitForElement("collectionView");

			// Get Y position of header
			var headerRect = App.WaitForElement("HeaderLabel").GetRect();
			var headerY = headerRect.Y;

			// Get initial Y position of "Person 0" 
			var initialRect = App.WaitForElement("Person 0").GetRect();
			var initialY = initialRect.Y;

			// Person 0 should be below the header initially
			Assert.That(initialY, Is.GreaterThan(headerY),
				"Person 0 should be below header initially");

			// Tap button to scroll to index 0 with ScrollToPosition.Start
			App.Tap("ScrollToFirstItemButton");
			Task.Delay(1000).Wait();

			// Get Y position after ScrollTo(0)
			var afterScrollToRect = App.WaitForElement("Person 0").GetRect();
			var afterScrollToY = afterScrollToRect.Y;

			Assert.That(afterScrollToY, Is.LessThan(initialY),
				$"ScrollTo(0) should scroll Person 0 to top, not push it down. " +
				$"Initial Y={initialY}, After ScrollTo Y={afterScrollToY}. " +
				$"If Y increased, the header was incorrectly treated as index 0.");
		}
	}
}
