using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class CollectionViewTabbedPageUITests : _IssuesUITest
	{
		public CollectionViewTabbedPageUITests(TestDevice device)
			: base(device)
		{
		}

		public override string Issue => "If CollectionView in other Tab gets changed before it's displayed, it stays invisible";

		/*
		const string Add1 = "Add1";
		const string Add2 = "Add2";
		const string Success = "Success";
		const string Tab2 = "Tab2";
		const string Tab3 = "Tab3";

		// AddingGroupToUnviewedGroupedCollectionViewShouldNotCrash (src\Compatibility\ControlGallery\src\Issues.Shared\Issue7700.cs)
		[Test]
		[FailsOnAllPlatforms("Click does not find Tab elements")]
		[Category(UITestCategories.CollectionView)]
		public void AddingItemToUnviewedCollectionViewShouldNotCrash()
		{
			App.WaitForElement(Add1);
			App.Click(Add1);
			App.Click(Tab2);

			App.WaitForElement(Success);
		}

		// AddingGroupToUnviewedGroupedCollectionViewShouldNotCrash (src\Compatibility\ControlGallery\src\Issues.Shared\Issue7700.cs)
		[Test]
		[FailsOnAllPlatforms("Click does not find Tab elements")]
		[Category(UITestCategories.CollectionView)]
		public void AddingGroupToUnviewedGroupedCollectionViewShouldNotCrash()
		{
			App.WaitForElement(Add2);
			App.Click(Add2);
			App.Click(Tab3);

			App.WaitForElement(Success);
		}
		*/
	}
}