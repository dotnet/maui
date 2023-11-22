using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.CollectionView)]
	public class CollectionViewEmptyViewStringUITests : CollectionViewUITests
	{
		public CollectionViewEmptyViewStringUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		public void StringEmptyViewAfterFilter()
		{
			App.Click("EmptyViewString");
			App.WaitForElement("TestCollectionView");

			// 1. Filter the items with a non existing term.
			App.EnterText("FilterSearchBar", "no exist");

			// 2. Check if the String EmptyView is visible.
			VerifyScreenshot();
		}

		[Test]
		public void FilterCollectionViewNoCrash()
		{
			App.Click("EmptyViewString");

			// 1. Filter the items with an existing term.
			App.EnterText("FilterSearchBar", "a");

			// 2. Without exceptions, the test has passed.
			Assert.NotNull(App.AppState);
		}

		[Test]
		public void RemoveStringEmptyView()
		{
			App.Click("EmptyViewString");
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