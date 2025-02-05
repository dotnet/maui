using NUnit.Framework;
using UITest.Appium;
using UITest.Core;

namespace Microsoft.Maui.TestCases.Tests
{
	public class VisitAndUpdateItemsSourceUITests : CollectionViewUITests
	{
		readonly string _collectionViewId = "collectionview";
		readonly string _btnUpdate = "btnUpdate";
		readonly string _entryUpdate = "entryUpdate";
		readonly string _entryInsert = "entryInsert";
		readonly string _entryRemove = "entryRemove";
		readonly string _entryReplace = "entryReplace";
		readonly string _btnInsert = "btnInsert";
		readonly string _btnRemove = "btnRemove";
		readonly string _btnReplace = "btnReplace";
		readonly string _inserted = "Inserted";
		readonly string _replaced = "Replacement";

		public VisitAndUpdateItemsSourceUITests(TestDevice device)
			: base(device)
		{
		}
		protected override bool ResetAfterEachTest => true;

		// VisitAndUpdateItemsSource (src\Compatibility\ControlGallery\src\UITests.Shared\Tests\CollectionViewUITests.cs)
		[TestCase("Observable Collection", "Add/RemoveItemsList", 19, 6)]
		[TestCase("Observable Collection", "Add/RemoveItemsGrid", 19, 6)]
		[TestCase("Default Text", "VerticalListCode", 101, 11)]
		[TestCase("Default Text", "HorizontalListCode", 101, 11)]
		[TestCase("Default Text", "VerticalGridCode", 101, 11)]
		[TestCase("Default Text", "HorizontalGridCode", 101, 11)]
		[TestCase("DataTemplate", "VerticalListCode", 19, 6)]
		[TestCase("DataTemplate", "HorizontalListCode", 19, 6)]
		[TestCase("DataTemplate", "VerticalGridCode", 19, 6)]
		[TestCase("DataTemplate", "HorizontalGridCode", 19, 6)]
		[Category(UITestCategories.CollectionView)]
		[FailsOnWindowsWhenRunningOnXamarinUITest("This test is failing, likely due to product issue.")]
		public void VisitAndUpdateItemsSource(string collectionTestName, string subGallery, int firstItem, int lastItem)
		{
			VisitInitialGallery(collectionTestName);
			VisitSubGallery(subGallery, !subGallery.Contains("Horizontal", StringComparison.OrdinalIgnoreCase), $"Item: {firstItem}", $"Item: {lastItem}", lastItem - 1, true, false);
		}

		void VisitSubGallery(string galleryName, bool scrollDown, string lastItem, string firstPageItem, int updateItemsCount, bool testItemSource, bool testAddRemove)
		{
			VisitSubGallery(galleryName);

			// Let's test the update
			if (testItemSource)
			{
				var collectionViewFrame = TestItemsExist(scrollDown, lastItem);
				TestUpdateItemsWorks(scrollDown, firstPageItem, updateItemsCount.ToString(), collectionViewFrame);
			}

			if (testAddRemove)
			{
				TestAddRemoveReplaceWorks(lastItem);
			}
		}

		void TestAddRemoveReplaceWorks(string lastItem)
		{
			App.WaitForElement(_entryRemove);
			App.ClearText(_entryRemove);
			App.EnterText(_entryRemove, "1");
			App.DismissKeyboard();
			App.WaitForElement(_btnRemove);
			App.Tap(_btnRemove);
			App.WaitForNoElement(lastItem);
			App.ClearText(_entryInsert);
			App.EnterText(_entryInsert, "1");
			App.DismissKeyboard();
			App.WaitForElement(_btnInsert);
			App.Tap(_btnInsert);
			App.WaitForElement(_inserted);
			//TODO: enable replace
			App.ClearText(_entryReplace);
			App.EnterText(_entryReplace, "1");
			App.DismissKeyboard();
			App.WaitForElement(_btnReplace);
			App.Tap(_btnReplace);
			App.WaitForElement(_replaced);
		}

		void TestUpdateItemsWorks(bool scrollDown, string itemMarked, string updateItemsCount, System.Drawing.Rectangle collectionViewFrame)
		{
			App.WaitForElement(_entryUpdate);

			//App.ScrollForElement($"* marked:'{itemMarked}'", new Drag(collectionViewFrame, scrollDown ? Drag.Direction.TopToBottom : Drag.Direction.LeftToRight, Drag.DragLength.Long), 50);
			//App.ScrollTo(itemMarked);

			App.ClearText(_entryUpdate);
			App.EnterText(_entryUpdate, updateItemsCount);
			App.DismissKeyboard();
			App.WaitForElement(_btnUpdate);
			App.Tap(_btnUpdate);
			App.WaitForNoElement(itemMarked);
		}

		System.Drawing.Rectangle TestItemsExist(bool scrollDown, string itemMarked)
		{
			App.WaitForElement(_btnUpdate);
			var collectionViewFrame = App.FindElement(_collectionViewId).GetRect();

			//App.ScrollForElement($"* marked:'{itemMarked}'", new Drag(collectionViewFrame, scrollDown ? Drag.Direction.BottomToTop : Drag.Direction.RightToLeft, Drag.DragLength.Long));
			//App.ScrollTo(itemMarked);

			return collectionViewFrame;
		}
	}
}