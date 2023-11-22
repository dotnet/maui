using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.CollectionView)]
	public class CollectionViewEmptyViewViewUITests : CollectionViewUITests
	{
		public CollectionViewEmptyViewViewUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		public void ViewEmptyViewAfterFilter()
		{
			App.Click("EmptyViewView");
			App.WaitForElement("TestCollectionView");

			// 1. Filter the items with a non existing term.
			App.EnterText("FilterSearchBar", "no exist");

			// 2. Check if the View EmptyView is visible.
			VerifyScreenshot();
		}

		[Test]
		public void RemoveViewEmptyView()
		{
			App.Click("EmptyViewView");
			App.WaitForElement("TestCollectionView");

			// 1. Filter the items with a non existing term.
			App.EnterText("FilterSearchBar", "no exist");

			// 2. Clear filter .
			App.EnterText("FilterSearchBar", "");

			// 3. Check if the CollectionView is visible.
			VerifyScreenshot();
		}
	}
}