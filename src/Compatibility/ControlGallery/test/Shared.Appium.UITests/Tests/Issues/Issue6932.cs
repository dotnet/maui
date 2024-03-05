using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace UITests
{
	public class Issue6932 : IssuesUITest
	{
		const string LayoutAutomationId = "StackLayoutThing";
		const string AddAutomationId = "AddButton";
		const string ClearAutomationId = "ClearButton";
		const string EmptyViewAutomationId = "EmptyViewId";

		public Issue6932(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "EmptyView for BindableLayout (view)";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void EmptyViewBecomesVisibleWhenItemsSourceIsCleared()
		{
			App.Screenshot("Screen opens, items are shown");

			App.WaitForElement(LayoutAutomationId);
			App.Click(ClearAutomationId);
			App.WaitForElement(EmptyViewAutomationId);

			App.Screenshot("Empty view is visible");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void EmptyViewHidesWhenItemsSourceIsFilled()
		{
			App.Screenshot("Screen opens, items are shown");

			App.WaitForElement(LayoutAutomationId);
			App.Click(ClearAutomationId);
			App.WaitForElement(EmptyViewAutomationId);

			App.Screenshot("Items are cleared, empty view visible");

			App.Click(AddAutomationId);
			App.WaitForNoElement(EmptyViewAutomationId);

			App.Screenshot("Item is added, empty view is not visible");
		}
	}
}