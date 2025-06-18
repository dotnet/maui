using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue6932 : _IssuesUITest
	{
		const string AddAutomationId = "AddButton";
		const string ClearAutomationId = "ClearButton";

		public Issue6932(TestDevice testDevice) : base(testDevice)
		{
		}

		public override string Issue => "EmptyView for BindableLayout (view)";

		[Test]
		[Category(UITestCategories.CollectionView)]
		[Category(UITestCategories.Compatibility)]
		public void EmptyViewBecomesVisibleWhenItemsSourceIsCleared()
		{
			App.WaitForAnyElement(["0", "1", "2", "3", "4", "5", "6", "7", "8", "9"]);
			App.WaitForElement(ClearAutomationId);
			App.Tap(ClearAutomationId);
			App.WaitForElement("No Results");
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void EmptyViewHidesWhenItemsSourceIsFilled()
		{
			App.WaitForElement(ClearAutomationId);
			App.Tap(ClearAutomationId);
			App.WaitForElement("No Results");
			App.WaitForElement(AddAutomationId);
			App.Tap(AddAutomationId);
			App.WaitForNoElement("No Results");
		}
	}
}