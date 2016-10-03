using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;

using Xamarin.Forms.CustomAttributes;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category("Cells")]
	internal class CellsGalleryTests : BaseTestFixture
	{
		// TODO find a way to test individula elements of cells
		// TODO port to new framework

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.CellsGalleryLegacy);
		}

		[Test]
		[Description("ListView with TextCells, all are present")]
		[UiTest(typeof(ListView))]
		[UiTest(typeof(TextCell))]
		public void CellsGalleryTextCellList()
		{
			App.ScrollForElement("* marked:'TextCell List'", new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
			App.Tap(q => q.Marked("TextCell List"));
			App.WaitForElement(q => q.Marked("Text 0"), "Timeout : Text 0");

			App.Screenshot("At TextCell List Gallery");

			App.ScrollForElement("* marked:'Detail 99'", new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.WaitForElement(q => q.Marked("Detail 99"), "Timeout : Detail 99");

			App.Screenshot("All TextCells are present");
		}

		[Test]
		[Description("TableView with TextCells, all are present")]
		[UiTest(typeof(TableView))]
		[UiTest(typeof(TextCell))]
		public void CellsGalleryTextCellTable()
		{
			App.ScrollForElement("* marked:'TextCell Table'", new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Tap(q => q.Marked("TextCell Table"));
			App.WaitForElement(q => q.Marked("Text 1"), "Timeout : Text 1");

			App.Screenshot("At TextCell Table Gallery");

			App.ScrollForElement("* marked:'Detail 12'", new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.WaitForElement(q => q.Marked("Detail 12"), "Timeout : Detail 12");

			App.Screenshot("All TextCells are present");
		}

		[Test]
		[Description("ListView with ImageCells, all are present")]
		[UiTest(typeof(ListView))]
		[UiTest(typeof(ImageCell))]
		public void CellsGalleryImageCellList()
		{
			Thread.Sleep(2000);

			App.ScrollForElement("* marked:'ImageCell List'", new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			Thread.Sleep(2000);

			App.Tap(q => q.Marked("ImageCell List"));
			App.WaitForElement(q => q.Marked("Text 0"), "Timeout : Text 0");

			App.Screenshot("At ImageCell List Gallery");

			var scollBounds = App.Query(q => q.Marked("ImageCellListView")).First().Rect;
			App.ScrollForElement("* marked:'Detail 99'", new Drag(scollBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.WaitForElement(q => q.Marked("Detail 99"), "Timeout : Detail 99");

			App.Screenshot("All ImageCells are present");

			var numberOfImages = App.Query(q => q.Raw(PlatformViews.Image)).Length;
			// Check that there are images present. In Android, 
			// have to make sure that there are more than 2 for navigation.
			Assert.IsTrue(numberOfImages > 2);

			App.Screenshot("Images are present");
		}

		[Test]
		[Description("ListView with ImageCells, file access problems")]
		[UiTest(typeof(ListView))]
		[UiTest(typeof(ImageCell))]
		public async Task CellsGalleryImageUrlCellList()
		{

			App.ScrollForElement("* marked:'ImageCell Url List'", new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Tap(q => q.Marked("ImageCell Url List"));

			//var scollBounds = App.Query(q => q.Marked("ImageUrlCellListView")).First().Rect;
			//App.ScrollForElement("* marked:'Detail 200'", new Drag(scollBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
			//App.ScrollUp();
			//App.WaitForElement(q => q.Marked("Detail 200"), "Timeout : Detail 200");

			App.Screenshot("All ImageCells are present");

			await Task.Delay(1000);
			var numberOfImages = App.Query(q => q.Raw(PlatformViews.Image)).Length;
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
			App.ScrollForElement("* marked:'ImageCell Table'", new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Tap(q => q.Marked("ImageCell Table"));
			App.WaitForElement(q => q.Marked("Text 1"), "Timeout : Text 1");

			App.Screenshot("At ImageCell Table Gallery");

			App.ScrollForElement("* marked:'Detail 12'", new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.WaitForElement(q => q.Marked("Detail 12"), "Timeout : Detail 12");

			App.Screenshot("All ImageCells are present");

			var numberOfImages = App.Query(q => q.Raw(PlatformViews.Image)).Length;
			// Check that there are images present. In Android, 
			// have to make sure that there are more than 2 for navigation.
			Assert.IsTrue(numberOfImages > 2);

			App.Screenshot("Images are present");
		}

		[Test]
		[Description("ListView with SwitchCells, all are present")]
		[UiTest(typeof(ListView))]
		[UiTest(typeof(SwitchCell))]
		public void CellsGallerySwitchCellList()
		{
			App.ScrollForElement("* marked:'SwitchCell List'", new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Tap(q => q.Marked("SwitchCell List"));
			App.WaitForElement(q => q.Marked("Label 0"), "Timeout : Label 0");

			App.Screenshot("At SwitchCell List Gallery");

			App.ScrollForElement("* marked:'Label 99'", new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			var numberOfSwitches = App.Query(q => q.Raw(PlatformViews.Switch)).Length;
			Assert.IsTrue(numberOfSwitches > 2);

			App.Screenshot("Switches are present");
		}

		[Test]
		[Description("TableView with SwitchCells, all are present")]
		[UiTest(typeof(TableView))]
		[UiTest(typeof(SwitchCell))]
		public void CellsGallerySwitchCellTable()
		{
			App.ScrollForElement("* marked:'SwitchCell Table'", new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Tap(q => q.Marked("SwitchCell Table"));
			App.WaitForElement(q => q.Marked("text 1"), "Timeout : text 1");

			App.Screenshot("At SwitchCell Table Gallery");

			App.ScrollForElement("* marked:'text 32'", new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			var numberOfSwitches = App.Query(q => q.Raw(PlatformViews.Switch)).Length;
			Assert.IsTrue(numberOfSwitches > 2);

			App.Screenshot("Switches are present");
		}

		[Test]
		[Description("ListView with EntryCells, all are present")]
		[UiTest(typeof(ListView))]
		[UiTest(typeof(EntryCell))]
		public void CellsGalleryEntryCellList()
		{
			App.ScrollForElement("* marked:'EntryCell List'", new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Tap(q => q.Marked("EntryCell List"));
			App.WaitForElement(q => q.Marked("Label 0"), "Timeout : Label 0");

			App.Screenshot("At EntryCell List Gallery");

			App.ScrollForElement("* marked:'Label 99'", new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Screenshot("All EntryCells are present");
		}

		[Test]
		[Description("TableView with EntryCells, all are present")]
		[UiTest(typeof(TableView))]
		[UiTest(typeof(EntryCell))]
		public void CellsGalleryEntryCellTable()
		{
			App.ScrollForElement("* marked:'EntryCell Table'", new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Tap(q => q.Marked("EntryCell Table"));
			App.WaitForElement(q => q.Marked("Text 2"), "Timeout : Text 2");

			App.Screenshot("At EntryCell Table Gallery");

			App.ScrollForElement("* marked:'Text 32'", new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Screenshot("All EntryCells are present");
		}

		[Test]
		[Category("EntryCell")]
		[Description("EntryCell fires .Completed event")]
		[UiTest(typeof(EntryCell), "Completed")]
		public void CellsGalleryEntryCellCompleted()
		{
			App.ScrollForElement("* marked:'EntryCell Table'", new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Tap(q => q.Marked("EntryCell Table"));
			App.WaitForElement(q => q.Marked("Text 2"), "Timeout : Text 2");

			App.Screenshot("At EntryCell Table Gallery");
			App.ScrollForElement("* marked:'Enter text'", new Drag(ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Screenshot("Before clicking Entry");

#if !__IOS__
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

