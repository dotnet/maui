using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CollectionViewTabbedPageUITests : _IssuesUITest
	{
		const string Add1 = "Add1";
		const string Add2 = "Add2";
		const string Success = "Success";
		const string Tab2 = "Tab2";
		const string Tab3 = "Tab3";
		protected override bool ResetAfterEachTest => true;

		public CollectionViewTabbedPageUITests(TestDevice device)
			: base(device)
		{
		}
		public override string Issue => "If CollectionView in other Tab gets changed before it's displayed, it stays invisible";

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void AddingItemToUnviewedCollectionViewShouldNotCrash()
		{
			App.WaitForElement(Add1);
			App.Tap(Add1);
			App.TapTab(Tab2);
			App.WaitForElementTillPageNavigationSettled(Success);
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void AddingGroupToUnviewedGroupedCollectionViewShouldNotCrash()
		{
			App.WaitForElement(Add2);
			App.Tap(Add2);
			App.TapTab(Tab3);
			App.WaitForElementTillPageNavigationSettled(Success);
		}
	}
}