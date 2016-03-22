using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

using NUnit.Framework;

using Xamarin.Forms.CustomAttributes;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("Cells")]
	internal class CellsGalleryTests : BaseTestFixture
	{
		// TODO find a way to test individula elements of cells
		// TODO port to new framework

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.CellsGalleryLegacy);
		}

		[Test]
		[Description ("ListView with TextCells, all are present")]
		[UiTest (typeof(ListView))]
		[UiTest (typeof(TextCell))]
		public void CellsGalleryTextCellList ()
		{
			App.ScrollForElement ("* marked:'TextCell List'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
			App.Tap (q => q.Marked ("TextCell List"));
			App.WaitForElement (q => q.Marked ("Text 0"), "Timeout : Text 0");

			App.Screenshot ("At TextCell List Gallery");

			App.ScrollForElement ("* marked:'Detail 99'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.WaitForElement (q => q.Marked ("Detail 99"), "Timeout : Detail 99");

			App.Screenshot ("All TextCells are present");
		}

		[Test]
		[Description ("TableView with TextCells, all are present")]
		[UiTest (typeof(TableView))]
		[UiTest (typeof(TextCell))]
		public void CellsGalleryTextCellTable ()
		{
			App.ScrollForElement ("* marked:'TextCell Table'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Tap (q => q.Marked ("TextCell Table"));
			App.WaitForElement (q => q.Marked ("Text 1"), "Timeout : Text 1");

			App.Screenshot ("At TextCell Table Gallery");

			App.ScrollForElement ("* marked:'Detail 12'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.WaitForElement (q => q.Marked ("Detail 12"), "Timeout : Detail 12");

			App.Screenshot ("All TextCells are present");
		}

		[Test]
		[Description ("ListView with ImageCells, all are present")]
		[UiTest (typeof(ListView))]
		[UiTest (typeof(ImageCell))]
		public void CellsGalleryImageCellList ()
		{
			Thread.Sleep (2000);

			App.ScrollForElement ("* marked:'ImageCell List'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			Thread.Sleep (2000);

			App.Tap (q => q.Marked ("ImageCell List"));
			App.WaitForElement (q => q.Marked ("Text 0"), "Timeout : Text 0");

			App.Screenshot ("At ImageCell List Gallery");

			var scollBounds = App.Query (q => q.Marked ("ImageCellListView")).First ().Rect;
			App.ScrollForElement ("* marked:'Detail 99'", new Drag (scollBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.WaitForElement (q => q.Marked ("Detail 99"), "Timeout : Detail 99");

			App.Screenshot ("All ImageCells are present");

			var numberOfImages = App.Query (q => q.Raw (PlatformViews.Image)).Length;
			// Check that there are images present. In Android, 
			// have to make sure that there are more than 2 for navigation.
			Assert.IsTrue (numberOfImages > 2);

			App.Screenshot ("Images are present");
		}

		[Test]
		[Description ("ListView with ImageCells, file access problems")]
		[UiTest (typeof(ListView))]
		[UiTest (typeof(ImageCell))]
		public void CellsGalleryImageUrlCellList ()
		{
		
			App.ScrollForElement ("* marked:'ImageCell Url List'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Tap (q => q.Marked ("ImageCell Url List"));
		
			var scollBounds = App.Query (q => q.Marked ("ImageUrlCellListView")).First ().Rect;
			App.ScrollForElement ("* marked:'Detail 200'", new Drag (scollBounds, Drag.Direction.BottomToTop, Drag.DragLength.Long), 40);

			App.WaitForElement (q => q.Marked ("Detail 200"), "Timeout : Detail 200");

			App.Screenshot ("All ImageCells are present");

			var numberOfImages = App.Query (q => q.Raw (PlatformViews.Image)).Length;
			// Check that there are images present. In Android, 
			// have to make sure that there are more than 2 for navigation.
			Assert.IsTrue (numberOfImages > 2);

			App.Screenshot ("Images are present");
		}

		


		[Test]
		[Description ("TableView with ImageCells, all are present")]
		[UiTest (typeof(TableView))]
		[UiTest (typeof(ImageCell))]
		public void CellsGalleryImageCellTable ()
		{
			App.ScrollForElement ("* marked:'ImageCell Table'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Tap (q => q.Marked ("ImageCell Table"));
			App.WaitForElement (q => q.Marked ("Text 1"), "Timeout : Text 1");

			App.Screenshot ("At ImageCell Table Gallery");

			App.ScrollForElement ("* marked:'Detail 12'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.WaitForElement (q => q.Marked ("Detail 12"), "Timeout : Detail 12");

			App.Screenshot ("All ImageCells are present");

			var numberOfImages = App.Query (q => q.Raw (PlatformViews.Image)).Length;
			// Check that there are images present. In Android, 
			// have to make sure that there are more than 2 for navigation.
			Assert.IsTrue (numberOfImages > 2);

			App.Screenshot ("Images are present");		
		}

		[Test]
		[Description ("ListView with SwitchCells, all are present")]
		[UiTest (typeof(ListView))]
		[UiTest (typeof(SwitchCell))]
		public void CellsGallerySwitchCellList ()
		{
			App.ScrollForElement ("* marked:'SwitchCell List'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Tap (q => q.Marked ("SwitchCell List"));
			App.WaitForElement (q => q.Marked ("Label 0"), "Timeout : Label 0");

			App.Screenshot ("At SwitchCell List Gallery");

			App.ScrollForElement ("* marked:'Label 99'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			var numberOfSwitches = App.Query (q => q.Raw (PlatformViews.Switch)).Length;
			Assert.IsTrue (numberOfSwitches > 2);

			App.Screenshot ("Switches are present");	
		}

		[Test]
		[Description ("TableView with SwitchCells, all are present")]
		[UiTest (typeof(TableView))]
		[UiTest (typeof(SwitchCell))]
		public void CellsGallerySwitchCellTable ()
		{
			App.ScrollForElement ("* marked:'SwitchCell Table'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Tap (q => q.Marked ("SwitchCell Table"));
			App.WaitForElement (q => q.Marked ("text 1"), "Timeout : text 1");

			App.Screenshot ("At SwitchCell Table Gallery");

			App.ScrollForElement ("* marked:'text 32'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			var numberOfSwitches = App.Query (q => q.Raw (PlatformViews.Switch)).Length;
			Assert.IsTrue (numberOfSwitches > 2);

			App.Screenshot ("Switches are present");
		}

		[Test]
		[Description ("ListView with EntryCells, all are present")]
		[UiTest (typeof(ListView))]
		[UiTest (typeof(EntryCell))]
		public void CellsGalleryEntryCellList ()
		{
			App.ScrollForElement ("* marked:'EntryCell List'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Tap (q => q.Marked ("EntryCell List"));
			App.WaitForElement (q => q.Marked ("Label 0"), "Timeout : Label 0");

			App.Screenshot ("At EntryCell List Gallery");

			App.ScrollForElement ("* marked:'Label 99'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Screenshot ("All EntryCells are present");
		}
			
		[Description ("Entered text stays after scrolled out of view")]
		[Issue (IssueTracker.Github, 1024, "EntryCell with text set clears after scrolling off screen", PlatformAffected.Android)]
		[UiTest (typeof(EntryCell))]
		public void CellsGalleryIssue1024 ()
		{
			// TODO fix
//			App.ScrollForElement ("* marked:'EntryCell List'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
//
//			App.Tap (q => q.Marked ("EntryCell List"));
//			App.WaitForElement (q => q.Marked ("Label 0"), "Timeout : Label 0");
//			App.Screenshot ("At EntryCell List Gallery");
//
//			App.Tap (Queries.EntryCellWithPlaceholder ("Placeholder 1"));
//			App.EnterText (Queries.EntryCellWithPlaceholder ("Placeholder 1"), "I am going to be scrolled off screen");
//
//			App.Screenshot ("Dismiss keyboard");
//
//			App.ScrollForElement ("* marked:'Label 40'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
//			App.Screenshot ("Scroll down");
//			
//			App.ScrollForElement ("* marked:'Label 0'", new Drag (ScreenBounds, Drag.Direction.TopToBottom, Drag.DragLength.Medium));
//
//			App.WaitForElement (Queries.EntryCellWithText ("I am going to be scrolled off screen"), "Timeout : Scrolled Entry with Text");
//			App.Screenshot ("Scroll back up to cell");
		}

		[Test]
		[Description ("TableView with EntryCells, all are present")]
		[UiTest (typeof(TableView))]
		[UiTest (typeof(EntryCell))]
		public void CellsGalleryEntryCellTable ()
		{
			App.ScrollForElement ("* marked:'EntryCell Table'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Tap (q => q.Marked ("EntryCell Table"));
			App.WaitForElement (q => q.Marked ("Text 2"), "Timeout : Text 2");

			App.Screenshot ("At EntryCell Table Gallery");

			App.ScrollForElement ("* marked:'Text 32'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Screenshot ("All EntryCells are present");
		}

		[Test]
		[Category ("EntryCell")]
		[Description ("EntryCell fires .Completed event")]
		[UiTest (typeof(EntryCell), "Completed")]
		public void CellsGalleryEntryCellCompleted ()
		{
			App.ScrollForElement ("* marked:'EntryCell Table'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Tap (q => q.Marked ("EntryCell Table"));
			App.WaitForElement (q => q.Marked ("Text 2"), "Timeout : Text 2");

			App.Screenshot ("At EntryCell Table Gallery");
			App.ScrollForElement ("* marked:'Enter text'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

			App.Screenshot ("Before clicking Entry");

			App.Tap (PlatformQueries.EntryCellWithPlaceholder ("I am a placeholder"));
			App.EnterText (PlatformQueries.EntryCellWithPlaceholder ("I am a placeholder"), "Hi");
			App.Screenshot ("Entered Text");
			if (App is AndroidApp) {
				((AndroidApp)App).PressUserAction (UserAction.Done);
			} else {
				App.PressEnter ();
			}
			App.WaitForElement (q => q.Marked ("Entered: 1"));
			App.Screenshot ("Completed should have changed label's text");
		}

		//[Test]
		[Description ("Issue 1033 - page does not respect orientation changes")]
		public void CellsGalleryIssue1033 ()
		{
//			App.SetOrientationLandscape ();
//			App.Tap (q => q.Marked ("TextCell List"));
//			App.WaitForElement (q => q.Marked ("Text 2"), "Timeout : Text 2");
//			float listViewHeightLandscape = App.Query (q => q.Raw (Views.ListView))[0].Rect.Height;
//			App.Screenshot ("Landscape list");
//
//
//			App.NavigateBack ();
//			App.Screenshot ("Navigate back");
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Set orientation to portrait");
//
//			App.Tap (q => q.Marked ("TextCell List"));
//			App.WaitForElement (q => q.Marked ("Text 2"), "Timeout : Text 2");
//			float listViewHeightPortrait = App.Query (q => q.Raw (Views.ListView))[0].Rect.Height;
//			App.Screenshot ("Portrait list");
//
//			// Should be be the same size if the layout is resized
//			Assert.AreNotEqual (listViewHeightLandscape, listViewHeightPortrait);
		}

		protected override void TestTearDown()
		{
			App.NavigateBack ();
			base.TestTearDown();
		}
	}
}

