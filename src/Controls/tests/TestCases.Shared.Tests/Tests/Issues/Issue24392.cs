#if IOS || MACCATALYST
using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue24392 : _IssuesUITest
	{
		public Issue24392(TestDevice testDevice) : base(testDevice) { }

		public override string Issue => "Items in CollectionView take up large vertical space in iOS";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void HorizontalCollectionViewItemsShouldNotTakeFullHeight()
		{
			// Wait for the CollectionView to be ready
			App.WaitForElement("TestCollectionView");

			// Get the CollectionView's bounding rectangle
			var collectionViewRect = App.WaitForElement("TestCollectionView").GetRect();

			// Get the header and content labels to verify layout
			var headerRect = App.WaitForElement("HeaderLabel").GetRect();
			var contentRect = App.WaitForElement("ContentLabel").GetRect();

			// The CollectionView should not take up the full remaining height
			// In the bug, items would expand to fill the entire viewport height
			// After the fix, the CollectionView height should be much smaller (just the item height)
			
			// The content label should be visible and have a reasonable height
			// If the bug exists, the CollectionView would push the content label off screen
			Assert.That(contentRect.Height, Is.GreaterThan(0), "Content label should be visible");
			
			// The CollectionView height should be much less than the total available space
			// A reasonable horizontal CollectionView with small items should not exceed 200 pixels in height
			Assert.That(collectionViewRect.Height, Is.LessThan(200), 
				$"CollectionView height ({collectionViewRect.Height}) should be small for horizontal layout with auto-sized items");

			// Verify screenshot to ensure visual correctness
			VerifyScreenshot();
		}
	}
}
#endif
