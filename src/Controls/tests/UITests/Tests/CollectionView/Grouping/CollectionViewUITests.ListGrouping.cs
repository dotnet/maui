using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.CollectionView)]
	public class CollectionViewListGroupingUITests : UITest
	{
		public CollectionViewListGroupingUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		[Description("A grouped list works and renders")]
		public void ListGrouping()
		{
			App.ScrollTo("ListGrouping", true);
			App.Click("ListGrouping");
			App.WaitForElement("TestCollectionView");

			// 1. Check the grouped CollectionView layout.
			VerifyScreenshot();
		}
	}
}