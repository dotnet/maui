#if ANDROID
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue16624 : _IssuesUITest
	{
		public Issue16624(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "SwipeGesture is not working on a ListView/CollectionView";

		[Test]
		[Category(UITestCategories.Gestures)]
		public void SwipeGestureOnCollectionViewShouldWork()
		{
			// Wait for the page to load
			App.WaitForElement("TestCollectionView");
			App.WaitForElement("StatusLabel");

			// Get the CollectionView element to perform swipe on
			var collectionView = App.WaitForElement("TestCollectionView");
			var rect = collectionView.GetRect();
			var centerX = rect.X + rect.Width / 2;
			var centerY = rect.Y + rect.Height / 2;

			// Perform a left swipe on the CollectionView
			App.DragCoordinates(centerX + 100, centerY, centerX - 100, centerY);

			// Wait for the status label to update
			App.WaitForElement("StatusLabel");

			// Verify the swipe was detected
			var statusLabel = App.WaitForElement("StatusLabel");
			var labelText = statusLabel.GetText();
			
			Assert.That(labelText, Does.Contain("Swiped"), "Swipe gesture should be detected on CollectionView");
		}

		[Test]
		[Category(UITestCategories.Gestures)]
		public void SwipeRightGestureOnCollectionViewShouldWork()
		{
			// Wait for the page to load
			App.WaitForElement("TestCollectionView");
			App.WaitForElement("StatusLabel");

			// Get the CollectionView element to perform swipe on
			var collectionView = App.WaitForElement("TestCollectionView");
			var rect = collectionView.GetRect();
			var centerX = rect.X + rect.Width / 2;
			var centerY = rect.Y + rect.Height / 2;

			// Perform a right swipe on the CollectionView
			App.DragCoordinates(centerX - 100, centerY, centerX + 100, centerY);

			// Wait for the status label to update
			App.WaitForElement("StatusLabel");

			// Verify the swipe was detected
			var statusLabel = App.WaitForElement("StatusLabel");
			var labelText = statusLabel.GetText();
			
			Assert.That(labelText, Does.Contain("Swiped"), "Swipe gesture should be detected on CollectionView");
		}
	}
}
#endif
