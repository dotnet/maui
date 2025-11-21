using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue32493 : _IssuesUITest
	{
		public override string Issue => "Selected item color changes from lightskyblue to lightgray after scrolling on iOS 26.1";

		public Issue32493(TestDevice device) : base(device) { }

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void SelectionColorShouldPersistAfterScrolling()
		{
			// Wait for CollectionView to be ready
			App.WaitForElement("CollectionView");

			// Tap on the first item to select it
			// Look for "Item 0" text in the collection view
			App.Tap("Item 0");

			// Wait a moment for selection to apply
			Task.Delay(500).Wait();

			// Scroll down to trigger cell reuse
			App.ScrollDown("CollectionView", ScrollStrategy.Gesture, swipeSpeed: 500);
			Task.Delay(500).Wait();

			// Scroll back up to see the selected item again
			App.ScrollUp("CollectionView", ScrollStrategy.Gesture, swipeSpeed: 500);
			Task.Delay(500).Wait();

			// Verify screenshot to check if color is still correct
			// The selected item should still be LightSkyBlue, not LightGray
			VerifyScreenshot();
		}
	}
}
