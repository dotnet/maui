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
			RunningApp.Screenshot("Screen opens, items are shown");

			RunningApp.WaitForElement(LayoutAutomationId);
			RunningApp.Tap(ClearAutomationId);
			RunningApp.WaitForElement(EmptyViewAutomationId);

			RunningApp.Screenshot("Empty view is visible");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnIOS]
		public void EmptyViewHidesWhenItemsSourceIsFilled()
		{
			RunningApp.Screenshot("Screen opens, items are shown");

			RunningApp.WaitForElement(LayoutAutomationId);
			RunningApp.Tap(ClearAutomationId);
			RunningApp.WaitForElement(EmptyViewAutomationId);

			RunningApp.Screenshot("Items are cleared, empty view visible");

			RunningApp.Tap(AddAutomationId);
			RunningApp.WaitForNoElement(EmptyViewAutomationId);

			RunningApp.Screenshot("Item is added, empty view is not visible");
		}
	}
}