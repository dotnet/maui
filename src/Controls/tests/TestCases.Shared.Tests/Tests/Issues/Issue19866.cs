using NUnit.Framework;
using NUnit.Framework.Legacy;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue19866 : _IssuesUITest
	{
		public Issue19866(TestDevice device) : base(device)
		{
		}

		public override string Issue => "[iOS] UICollectionView ScrollToTop does not work";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewScrollsToTopOnStatusBarTap()
		{
			// Wait for the CollectionView to be present
			App.WaitForElement("TestCollectionView");
			
			// Verify the CollectionView is present and contains items
			var collectionView = App.WaitForElement("TestCollectionView");
			ClassicAssert.IsNotNull(collectionView, "CollectionView should be present");
			
			// Verify the first item is initially visible
			App.WaitForElement("Item_1");
			
			// Scroll down to move away from the top
			App.ScrollDown("TestCollectionView");
			App.ScrollDown("TestCollectionView");
			
			// Wait for scroll to complete
			System.Threading.Thread.Sleep(500);
			
			// Simulate tapping the status bar area (top of the screen)
			// On iOS, the status bar is typically at the very top of the screen
			App.TapCoordinates(200, 10);
			
			// Wait for the scroll-to-top animation to complete
			System.Threading.Thread.Sleep(1000);
			
			// Verify we're back at the top by checking if the first item is visible
			var firstItem = App.WaitForElement("Item_1");
			ClassicAssert.IsNotNull(firstItem, "First item should be visible after status bar tap");
		}
	}
}