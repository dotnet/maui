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
#if ANDROID
		const string FirstPage = "7700 FIRST PAGE";
		const string Tab2 = "TAB2";
		const string Tab3 = "TAB3";
#else
		const string FirstPage = "7700 First Page";
		const string Tab2 = "Tab2";
		const string Tab3 = "Tab3";
#endif
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
			App.WaitForElement(Tab2);
			App.Tap(Tab2);
			App.WaitForElementTillPageNavigationSettled(Success);		
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void AddingGroupToUnviewedGroupedCollectionViewShouldNotCrash()
		{
			App.WaitForElement(Add2);
			App.Tap(Add2);
			App.WaitForElement(Tab3);
			App.Tap(Tab3);
			App.WaitForElementTillPageNavigationSettled(Success);
		}		
	}
}