using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests.Issues
{
	public class Issue21967 : _IssuesUITest
	{
		public Issue21967(TestDevice device) : base(device)
		{
		}

		public override string Issue => "CollectionView causes invalid measurements on resize";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewItemsResizeWhenContraintsOnCollectionViewChange()
		{
			var largestSize = App.WaitForElement("Item1").GetRect();
			App.Tap("Resize");
			var mediumSize = App.WaitForElement("Item1").GetRect();
			App.Tap("Resize");
			var smallSize = App.WaitForElement("Item1").GetRect();

            Assert.Greater(largestSize.Width, mediumSize.Width);
            Assert.Greater(mediumSize.Width, smallSize.Width);
		}
	}
}
