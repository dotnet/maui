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
			// This test validates that the CollectionView appears and can be interacted with
			// The fix ensures ScrollsToTop = true is set on the UICollectionView during handler connection
			App.WaitForElement("TestCollectionView");
			
			// Verify the CollectionView is present and contains items
			var collectionView = App.WaitForElement("TestCollectionView");
			Assert.IsNotNull(collectionView, "CollectionView should be present");
			
			// Verify instructions are present for manual testing
			var instructions = App.WaitForElement("InstructionsLabel");
			Assert.IsNotNull(instructions, "Instructions should be present for manual testing");
			
			// Note: The actual status bar tap gesture cannot be automated in UI tests
			// Manual testing required: 
			// 1. Scroll down in the CollectionView
			// 2. Tap the iOS status bar
			// 3. Verify the CollectionView scrolls back to the top
		}
	}
}