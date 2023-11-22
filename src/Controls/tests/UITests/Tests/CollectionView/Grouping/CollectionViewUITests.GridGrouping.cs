using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.CollectionView)]
	public class CollectionViewGridGroupingUITests : UITest
	{
		public CollectionViewGridGroupingUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		public void GridGrouping()
		{
			App.ScrollTo("GridGrouping", true);
			App.Click("GridGrouping");
			App.WaitForElement("TestCollectionView");

			// 1. Check the grouped CollectionView layout.
			VerifyScreenshot();
		}
	}
}