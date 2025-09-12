using NUnit.Framework;
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
		public void CollectionViewScrollsToTopIsEnabledOnIOS()
		{
			// Wait for the CollectionView to be present
			App.WaitForElement("TestCollectionView");
			
			// Verify the CollectionView is present and contains items
			var collectionView = App.WaitForElement("TestCollectionView");
			Assert.IsNotNull(collectionView, "CollectionView should be present");
			
			// Test scrolling functionality to ensure the CollectionView works properly
			// Scroll down to verify scrolling works
			App.ScrollDown("TestCollectionView");
			
			// Use the scroll to top button to test programmatic scroll to top
			var scrollToTopButton = App.WaitForElement("ScrollToTopButton");
			App.Tap("ScrollToTopButton");
			
			// Wait a moment for the scroll animation to complete
			System.Threading.Thread.Sleep(500);
			
			// Verify we can still interact with the first item (indicating we're at the top)
			var firstItem = App.WaitForElement("Item_1");
			Assert.IsNotNull(firstItem, "First item should be visible after scrolling to top");
			
			// Note: The actual status bar tap gesture cannot be automated in UI tests
			// The fix ensures ScrollsToTop = true is set on the UICollectionView during handler connection
			// Manual testing still required for status bar tap functionality
		}
	}
}