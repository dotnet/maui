using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.CollectionView)]
	public class CollectionViewHeaderFooterViewUITests : CollectionViewUITests
	{
		public CollectionViewHeaderFooterViewUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		public void ViewHeaderFooter()
		{
			this.IgnoreIfPlatforms(new TestDevice[] { TestDevice.iOS },
				"Currently fails on Android.");

			App.ScrollTo("HeaderFooterView", true);
			App.Click("HeaderFooterView");

			// 1. Both Header and Footer must be visible with or without items.
			// Let's add items.
			App.Click("AddButton");

			// 2. Clear the items.
			App.Click("ClearButton");

			// 3. Repeat the previous steps.
			App.Click("AddButton");
			App.Click("ClearButton");

			// 3. Check CollectionView header and footer using a View.
			App.WaitForElement("TestCollectionView");
			VerifyScreenshot();
		}
	}
}