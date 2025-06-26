using Xunit;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests.Issues
{
	public class Issue17969 : _IssuesUITest
	{

		public Issue17969(TestDevice device)
		: base(device)
		{ }

		public override string Issue => "CollectionView duplicates group headers/footers when adding a new item to a group or crashes when adding a new group with empty view";

		[Fact]
		[Trait("Category", UITestCategories.CollectionView)]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void CollectionViewDuplicateViewsWhenAddItemToGroup()
		{
			App.WaitForElement("collectionView");
			App.Tap("addItem");
			VerifyScreenshot();

		}

		[Fact]
		[Trait("Category", UITestCategories.CollectionView)]
		[FailsOnWindowsWhenRunningOnXamarinUITest]
		public void CollectionViewAddGroupWhenViewIsEmpty()
		{
			App.WaitForElement("collectionView");
			App.Tap("addGroup");
			VerifyScreenshot();
		}
	}
}