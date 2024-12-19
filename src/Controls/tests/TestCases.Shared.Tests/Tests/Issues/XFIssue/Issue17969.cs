#if TEST_FAILS_ON_WINDOWS    //Clicking the AddItem and AddGroup buttons results in a System.Runtime.InteropServices.COMException: 'No installed components were detected.'
using NUnit.Framework;
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

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewDuplicateViewsWhenAddItemToGroup()
		{
			App.WaitForElement("collectionView");
			App.Tap("addItem");
#if MACCATALYST
			App.WaitForElement("Asian Black Bear");
#else
			VerifyScreenshot();
#endif
		}

		[Test]
		[Category(UITestCategories.CollectionView)]
		public void CollectionViewAddGroupWhenViewIsEmpty()
		{
			App.WaitForElement("collectionView");
			App.Tap("addGroup");
#if MACCATALYST
			App.WaitForElement("Count: 1");
#else
			VerifyScreenshot();
#endif
		}
	}
}
#endif
