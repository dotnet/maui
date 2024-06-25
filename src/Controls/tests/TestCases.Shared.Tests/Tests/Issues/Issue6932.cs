using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue6932 : _IssuesUITest
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
		[Category(UITestCategories.Compatibility)]
		[FailsOnWindows]
		public void EmptyViewBecomesVisibleWhenItemsSourceIsCleared()
		{
			App.Screenshot("Screen opens, items are shown");

			App.WaitForElement(LayoutAutomationId);
			App.Tap(ClearAutomationId);
			App.WaitForElement(EmptyViewAutomationId);

			App.Screenshot("Empty view is visible");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnIOS]
		[FailsOnMac]
		[FailsOnWindows]
		public void EmptyViewHidesWhenItemsSourceIsFilled()
		{
			App.Screenshot("Screen opens, items are shown");

			App.WaitForElement(LayoutAutomationId);
			App.Tap(ClearAutomationId);
			App.WaitForElement(EmptyViewAutomationId);

			App.Screenshot("Items are cleared, empty view visible");

			App.Tap(AddAutomationId);
			App.WaitForNoElement(EmptyViewAutomationId);

			App.Screenshot("Item is added, empty view is not visible");
		}
	}
}