#if TEST_FAILS_ON_WINDOWS // StackLayout AutomationId and EmptyView are not accessible through appium.
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
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void EmptyViewBecomesVisibleWhenItemsSourceIsCleared()
		{
			App.WaitForElement(LayoutAutomationId);
			App.WaitForElement(ClearAutomationId);
			App.Tap(ClearAutomationId);
			App.WaitForElement(EmptyViewAutomationId);
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void EmptyViewHidesWhenItemsSourceIsFilled()
		{
			App.WaitForElement(LayoutAutomationId);
			App.WaitForElement(ClearAutomationId);
			App.Tap(ClearAutomationId);
			App.WaitForElement(EmptyViewAutomationId);
			App.WaitForElement(AddAutomationId);
			App.Tap(AddAutomationId);
			App.WaitForNoElement(EmptyViewAutomationId);
		}
	}
}
#endif