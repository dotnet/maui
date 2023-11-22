using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.AppiumTests
{
	[Category(TestCategory.CollectionView)]
	public class CollectionViewEmptyViewTemplateViewUITests : CollectionViewUITests
	{
		public CollectionViewEmptyViewTemplateViewUITests(TestDevice device)
			: base(device)
		{
		}

		[Test]
		[Description("Can use a TemplatedView as EmptyView")]
		public void TemplateEmptyViewAfterFilter()
		{
			App.Click("EmptyViewTemplateView");
			App.WaitForElement("TestCollectionView");

			// 1. Filter the items with a non existing term.
			App.EnterText("FilterSearchBar", "no exist");

			// 2. Check if the Templated EmptyView is visible.
			VerifyScreenshot();
		}

		[Test]
		public void RemoveTemplateViewEmptyView()
		{
			App.Click("EmptyViewTemplateView");
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