using NUnit.Framework;
using NUnit.Framework.Legacy;
using OpenQA.Selenium;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
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

            ClassicAssert.Greater(largestSize.Width, mediumSize.Width);
			ClassicAssert.Greater(mediumSize.Width, smallSize.Width);
		}
        
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewFirstItemCorrectlySetsTheMeasure()
		{
			var itemSize = App.WaitForElement("Item1").GetRect();
			ClassicAssert.Greater(200, itemSize.Height);
		}
        
		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewWorksWhenRotatingDevice()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.Mac, TestDevice.Windows });

			try
			{
				App.WaitForElement("FullSize");
				App.Tap("FullSize");
				App.SetOrientationPortrait();
				var itemSizePortrait = App.WaitForElement("Item1").GetRect();
				App.SetOrientationLandscape();
				var itemSizeLandscape = App.WaitForElement("Item1").GetRect();
				App.SetOrientationPortrait();
				var itemSizePortrait2 = App.WaitForElement("Item1").GetRect();

				ClassicAssert.Greater(itemSizeLandscape.Width, itemSizePortrait.Width);
				ClassicAssert.AreEqual(itemSizePortrait2.Width, itemSizePortrait.Width);
			}
			finally
			{
				App.SetOrientationPortrait();
			}
		}
	}
}