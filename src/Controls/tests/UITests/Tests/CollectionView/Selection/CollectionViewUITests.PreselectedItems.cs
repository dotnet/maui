using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.CollectionView)]
	public class CollectionViewPreselectedItemsUITests : CollectionViewUITests
	{
		public CollectionViewPreselectedItemsUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		public void PreselectedItemsCollectionView()
		{
			App.Click("PreselectedItems");
			App.WaitForElement("TestCollectionView");

			// 1. Check the preselected items.
			App.WaitForElement("WaitForStubControl");
			VerifyScreenshot();
		}
	}
}