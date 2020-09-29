using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Core.UITests
{
#if __MACOS__
	[Ignore("Not tested on the MAC")]
#endif
	[TestFixture]
	[Category(UITestCategories.Cells)]
	[Category(UITestCategories.UwpIgnore)]
	internal class CellsGalleryTests : BaseTestFixture
	{
		public const string CellTestContainerId = "CellTestContainer";

		// TODO find a way to test individual elements of cells
		// TODO port to new framework

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.CellsGalleryLegacy);
		}

		void SelectTest(string testName)
		{
#if __WINDOWS__
			App.ScrollDownTo(testName);
#else
			App.ScrollForElement($"* marked:'{testName}'",
				new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
#endif
			App.WaitForElement(q => q.Marked(testName));

			// This code was added to work around an issue
			// UI Test was having clicking the first two items in the List
			for (int i = 0; i < 5 && App.Query(q => q.Marked(testName)).Length > 0; i++)
			{
				App.Tap(q => q.Marked(testName));
				Thread.Sleep(500);
			}
		}

		[Test]
		[Description("ListView with TextCells, all are present")]
		[UiTest(typeof(ListView))]
		[UiTest(typeof(TextCell))]
		public void CellsGalleryTextCellList()
		{
			SelectTest("TextCell List");

			App.WaitForElement(q => q.Marked("Text 0"), "Timeout : Text 0");

			App.Screenshot("At TextCell List Gallery");

			string target = "Detail 99";

#if __WINDOWS__
			App.ScrollDownTo(target, CellTestContainerId, timeout: TimeSpan.FromMinutes(1));
#else
			App.ScrollForElement($"* marked:'{target}'",
				new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
#endif

			App.WaitForElement(q => q.Marked(target), $"Timeout : {target}");

			App.Screenshot("All TextCells are present");
		}

		[Test]
		[Description("TableView with TextCells, all are present")]
		[UiTest(typeof(TableView))]
		[UiTest(typeof(TextCell))]
		public void CellsGalleryTextCellTable()
		{
			SelectTest("TextCell Table");

			App.WaitForElement(q => q.Marked("Text 1"), "Timeout : Text 1");

			App.Screenshot("At TextCell Table Gallery");

			string target = "Detail 12";

#if __WINDOWS__
			App.ScrollDownTo(target, CellTestContainerId, timeout: TimeSpan.FromMinutes(1));
#else
			App.ScrollForElement($"* marked:'{target}'",
				new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
#endif

			App.WaitForElement(q => q.Marked(target), $"Timeout : {target}");

			App.Screenshot("All TextCells are present");
		}

		[Test]
		[Description("ListView with ImageCells, all are present")]
		[UiTest(typeof(ListView))]
		[UiTest(typeof(ImageCell))]
		public void CellsGalleryImageCellList()
		{
			Thread.Sleep(2000);

			SelectTest("ImageCell List");

			Thread.Sleep(2000);

			App.WaitForElement(q => q.Marked("Text 0"), "Timeout : Text 0");

			App.Screenshot("At ImageCell List Gallery");

			string target = "Detail 99";

#if __WINDOWS__
			App.ScrollDownTo(target, CellTestContainerId, timeout: TimeSpan.FromMinutes(3));
#else
			var scrollBounds = App.Query(q => q.Marked(CellTestContainerId)).First().Rect;
			App.ScrollForElement($"* marked:'{target}'",
				new Drag(scrollBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
#endif

			App.WaitForElement(q => q.Marked(target), $"Timeout : {target}");

			App.Screenshot("All ImageCells are present");

#if !__WINDOWS__
			var numberOfImages = App.Query(q => q.Raw(PlatformViews.Image)).Length;
			// Check that there are images present. In Android, 
			// have to make sure that there are more than 2 for navigation.
			Assert.IsTrue(numberOfImages > 2);
#endif

			App.Screenshot("Images are present");
		}

		[Test]
		[Description("ListView with ImageCells, file access problems")]
		[UiTest(typeof(ListView))]
		[UiTest(typeof(ImageCell))]
		public async Task CellsGalleryImageUrlCellList()
		{
			SelectTest("ImageCell Url List");

			App.WaitForElement(q => q.Marked("ImageUrlCellListView"));

			var scollBounds = App.Query(q => q.Marked("ImageUrlCellListView")).First().Rect;
			App.ScrollForElement("* marked:'Detail 100'", new Drag(scollBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
			App.WaitForElement(q => q.Marked("Detail 100"), "Timeout : Detail 100");

			App.Screenshot("All ImageCells are present");

			int numberOfImages = 0;

			// Most of the time, 1 second is long enough to wait for the images to load, but depending on network conditions
			// it may take longer
			for (int n = 0; n < 30; n++)
			{
				await Task.Delay(1000);
				numberOfImages = App.Query(q => q.Raw(PlatformViews.Image)).Length;
				if (numberOfImages > 2)
				{
					break;
				}
			}

			// Check that there are images present. In Android, 
			// have to make sure that there are more than 2 for navigation.
			Assert.IsTrue(numberOfImages > 2);

			App.Screenshot("Images are present");
		}

		[Test]
		[Description("TableView with ImageCells, all are present")]
		[UiTest(typeof(TableView))]
		[UiTest(typeof(ImageCell))]
		public void CellsGalleryImageCellTable()
		{
			SelectTest("ImageCell Table");

			App.WaitForElement(q => q.Marked("Text 1"), "Timeout : Text 1");

			App.Screenshot("At ImageCell Table Gallery");

			string target = "Detail 12";

#if __WINDOWS__
			App.ScrollDownTo(target, CellTestContainerId, timeout: TimeSpan.FromMinutes(1));
#else
			App.ScrollForElement($"* marked:'{target}'",
				new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
#endif

			App.WaitForElement(q => q.Marked(target), $"Timeout : {target}");

			App.Screenshot("All ImageCells are present");

#if !__WINDOWS__
			var numberOfImages = App.Query(q => q.Raw(PlatformViews.Image)).Length;
			// Check that there are images present. In Android, 
			// have to make sure that there are more than 2 for navigation.
			Assert.IsTrue(numberOfImages > 2);
#endif

			App.Screenshot("Images are present");
		}

		[Test]
		[Description("ListView with SwitchCells, all are present")]
		[UiTest(typeof(ListView))]
		[UiTest(typeof(SwitchCell))]
		public void CellsGallerySwitchCellList()
		{
			SelectTest("SwitchCell List");

			App.WaitForElement(q => q.Marked("Label 0"), "Timeout : Label 0");

			App.Screenshot("At SwitchCell List Gallery");

			string target = "Label 99";

#if __WINDOWS__
			App.ScrollDownTo(target, CellTestContainerId, timeout: TimeSpan.FromMinutes(1));
#else
			App.ScrollForElement($"* marked:'{target}'",
				new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.WaitForElement(q => q.Marked(target));

			var numberOfSwitches = App.Query(q => q.Raw(PlatformViews.Switch)).Length;
			Assert.IsTrue(numberOfSwitches > 2);
#endif

			App.Screenshot("Switches are present");
		}

		[Test]
		[Description("TableView with SwitchCells, all are present")]
		[UiTest(typeof(TableView))]
		[UiTest(typeof(SwitchCell))]
		public void CellsGallerySwitchCellTable()
		{
			SelectTest("SwitchCell Table");

			App.WaitForElement(q => q.Marked("text 1"), "Timeout : text 1");

			App.Screenshot("At SwitchCell Table Gallery");

			string target = "text 32";

#if __WINDOWS__
			App.ScrollDownTo(target, CellTestContainerId, timeout: TimeSpan.FromMinutes(1));
#else
			App.ScrollForElement($"* marked:'{target}'",
				new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.WaitForElement(q => q.Marked(target));

			var numberOfSwitches = App.Query(q => q.Raw(PlatformViews.Switch)).Length;
			Assert.IsTrue(numberOfSwitches > 2);
#endif

			App.Screenshot("Switches are present");
		}

		[Test]
		[Description("ListView with EntryCells, all are present")]
		[UiTest(typeof(ListView))]
		[UiTest(typeof(EntryCell))]
		public void CellsGalleryEntryCellList()
		{
			SelectTest("EntryCell List");

			App.WaitForElement(q => q.Marked("Label 0"), "Timeout : Label 0");

			App.Screenshot("At EntryCell List Gallery");

			string target = "Label 99";

#if __WINDOWS__
			App.ScrollDownTo(target, CellTestContainerId, timeout: TimeSpan.FromMinutes(3));
#else
			App.ScrollForElement($"* marked:'{target}'",
				new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
#endif

			App.Screenshot("All EntryCells are present");
		}

		[Test]
		[Description("TableView with EntryCells, all are present")]
		[UiTest(typeof(TableView))]
		[UiTest(typeof(EntryCell))]
		public void CellsGalleryEntryCellTable()
		{
			SelectTest("EntryCell Table");

			App.WaitForElement(q => q.Marked("Text 2"), "Timeout : Text 2");

			App.Screenshot("At EntryCell Table Gallery");

			string target = "Text 32";

#if __WINDOWS__
			App.ScrollDownTo(target, CellTestContainerId, timeout: TimeSpan.FromMinutes(1));
#else
			App.ScrollForElement($"* marked:'{target}'",
				new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
#endif

			App.Screenshot("All EntryCells are present");
		}

		[Test]
		[Category(UITestCategories.Cells)]
		[Description("EntryCell fires .Completed event")]
		[UiTest(typeof(EntryCell), "Completed")]
		public void CellsGalleryEntryCellCompleted()
		{
			SelectTest("EntryCell Table");

			App.WaitForElement(q => q.Marked("Text 2"), "Timeout : Text 2");

			App.Screenshot("At EntryCell Table Gallery");

			string target = "Enter text";

#if __WINDOWS__
			App.ScrollDownTo(target, CellTestContainerId, timeout: TimeSpan.FromMinutes(1));
#else
			App.ScrollForElement($"* marked:'{target}'",
				new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
#endif

			App.WaitForElement(q => q.Marked(target));

			App.Screenshot("Before clicking Entry");

#if !__IOS__ && !__WINDOWS__
			App.Tap(PlatformQueries.EntryCellWithPlaceholder("I am a placeholder"));
			App.EnterText(PlatformQueries.EntryCellWithPlaceholder("I am a placeholder"), "Hi");
			App.Screenshot("Entered Text");
			App.PressEnter();

			App.WaitForElement(q => q.Marked("Entered: 1"));
			App.Screenshot("Completed should have changed label's text");
			
#endif
		}

		protected override void TestTearDown()
		{
			App.NavigateBack();
			base.TestTearDown();
		}
	}
}