using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using System;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("Templated")]
	internal class TemplatedTabPageGalleryTests : BaseTestFixture
	{
		// TODO
		// TODO: Port to new conventions

		public TemplatedTabPageGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.TemplatedTabbedPageGalleryLegacy);
		}

//		void AllElementsExist ()
//		{
//			var labels = new [] { 
//				"Lorem ipsum dolor sit amet #0",
//				"Page 0",
//				"Page 1",
//			};

//			foreach (var label in labels)
//				App.WaitForElement (q => q.Marked (label), "Timeout : " + label);
				
//			AllElementExistForPage (0);

//			App.Tap (q => q.Marked ("Page 1"));
//			AllElementExistForPage (1);

//			App.Screenshot ("All elements exist");
//		}

		[Test]
		[Description ("Insert tab")]
		public void TemplatedTabPageGalleryInsertTab () 
		{
//			AllElementsExist ();

//			App.Tap (q => q.Button ("Insert Tab: 1"));
//			App.WaitForElement (q => q.Marked ("Page 2"), "Timeout : Page 2");

//			App.Screenshot ("Page 2 added");

//			App.Tap (q => q.Marked ("Page 2"));
//			AllElementExistForPage (2);

//			App.Screenshot ("Page 2 selected");
		}

//		[Test]
//		[Description ("Insert tab crash reproduction")]
//		public void TemplatedTabPageGalleryInsertTabCrash ()
//		{
//			TemplatedTabPageGalleryInsertTab ();
//			App.Tap (q => q.Marked ("Page 1"));
//			AllElementExistForPage (1);

//			App.Tap (q => q.Marked ("Page 0"));
//			AllElementExistForPage (0);
//		}

//		[Test]
//		[Description ("Change tab Title")]
//		public void TemplatedTabPageGalleryChangeTitles () 
//		{
//			AllElementsExist ();
//			App.Tap (q => q.Marked ("Change title: 1"));
//			App.WaitForNoElement (q => q.Marked ("Page 1"), "Timeout : Page 1");

//			// Change Page 1 title
//			for (int i = 0; i < 3; i++) {
//				App.WaitForElement (q => q.Marked ("Title: " + i), "Timeout : Title " + i);
//				App.Tap (q => q.Marked ("Change title: 1"));
//			}

//			App.Screenshot ("Page 1 titles changed");

//			// Change Page 0 title
//			App.Tap (q => q.Marked ("Page 0"));
//			App.Tap (q => q.Button ("Change title: 0"));
//			App.WaitForNoElement (q => q.Marked ("Page 0"), "Timeout : Page 0");

//			for (int i = 0; i < 3; i++) {
//				App.WaitForElement (q => q.Marked ("Title: " + i), "Timeout : Title " + i);
//				App.Tap (q => q.Button ("Change title: 0"));
//			}

//			App.Screenshot ("Page 0 titles changed");
//		}

//		[Test]
//		[Description ("Move tabs")]
//		public void TemplatedTabPageGalleryMoveTabs () 
//		{
//			AllElementsExist ();

//			int pageZeroTabIndex = App.IndexForElementWithText (PlatformQueries.Labels, "Page 0");
//			int pageOneTabIndex = App.IndexForElementWithText (PlatformQueries.Labels, "Page 1");

//			// Elements found
//			Assert.AreNotEqual (-1, pageZeroTabIndex);
//			Assert.AreNotEqual (-1, pageOneTabIndex);

//			Assert.Greater (pageOneTabIndex, pageZeroTabIndex);

//			App.Screenshot ("Tabs before move");

//			App.Tap (q => q.Button ("Move Tab: 1"));

//			int pageZeroMovedTabIndex = App.IndexForElementWithText (PlatformQueries.Labels, "Page 0");
//			int pageOneMovedTabIndex = App.IndexForElementWithText (PlatformQueries.Labels, "Page 1");

//			// Elements found
//			Assert.AreNotEqual (-1, pageZeroMovedTabIndex);
//			Assert.AreNotEqual (-1, pageOneMovedTabIndex);

//			Assert.Greater (pageZeroMovedTabIndex, pageOneMovedTabIndex);

//			App.Screenshot ("Tabs after move");
//		}

//		[Test]
//		[Description ("Remove tabs")]
//		public void TemplatedTabPageGalleryRemoveTabs () 
//		{
//			AllElementsExist ();

//			App.Tap (q => q.Button ("Remove Tab: 1"));
//			App.WaitForNoElement (q => q.Marked ("Page 1"), "Timeout : Page 1");

//			App.Screenshot ("Remove Page 1");
//		}

//		[Test]
//		[Description ("Add / remove tabs")]
//		public void TemplatedTabPageGalleryAddRemoveTabs () 
//		{
//			TemplatedTabPageGalleryInsertTab ();

//			App.Tap (q => q.Button ("Remove Tab: 2"));
//			App.WaitForNoElement (q => q.Marked ("Page 2"), "Timeout : Page 2");

//			App.Screenshot ("Remove Page 2");

//			App.Tap (q => q.Button ("Remove Tab: 0"));
//			App.WaitForNoElement (q => q.Marked ("Page 0"), "Timeout : Page 0");

//			App.Screenshot ("Remove Page 0");

//			AllElementExistForPage (1);
//		}

//		[Test]
//		[Description ("Reset tabs")]
//		public void TemplatedTabPageGalleryResetAllTabs () 
//		{
//			TemplatedTabPageGalleryChangeTitles ();

//			App.Tap (q => q.Button ("Insert Tab: 0"));
//			App.WaitForElement (q => q.Marked ("Page 2"), "Timeout : Page 2");

//			App.Screenshot ("Page 2 added");

//			App.Tap (q => q.Marked ("Page 2"));
//			AllElementExistForPage (2);

//			App.Screenshot ("Page 2 selected");

//			App.ScrollDownForElement (q => q.Button ("Reset all tabs: 2"), 3);
//			App.Tap (q => q.Button ("Reset all tabs: 2"));
//			App.WaitForElement (q => q.Marked ("Page 0"), "Timeout : Page 0");
//			App.WaitForElement (q => q.Marked ("Page 1"), "Timeout : Page 1");

//			var numberOfTabs = App.Query (q => q.Raw (PlatformStrings.Label + " {text BEGINSWITH 'Page'}")).Length;
//			Assert.AreEqual (2, numberOfTabs);

//			App.Screenshot ("Tabs reset");
//		}

//		[Test]
//		[Description ("Go to next tabs")]
//		public void TemplatedTabPageGalleryNextPage () 
//		{
//			TemplatedTabPageGalleryInsertTab ();

//			ScrollDownForQuery (q => q.Button ("Next Page: 2"));
//			App.Tap (q => q.Button ("Next Page: 2"));
//			AllElementExistForPage (0);
//			App.Screenshot ("On Page 0");

//			ScrollDownForQuery (q => q.Button ("Next Page: 0"));
//			App.Tap (q => q.Button ("Next Page: 0"));
//			AllElementExistForPage (1);
//			App.Screenshot ("On Page 1");

//			ScrollDownForQuery (q => q.Button ("Next Page: 1"));
//			App.Tap (q => q.Button ("Next Page: 1"));
//			AllElementExistForPage (2);
//			App.Screenshot ("On Page 2");

//			ScrollDownForQuery (q => q.Button ("Next Page: 2"));
//			App.Tap (q => q.Button ("Next Page: 2"));
//			AllElementExistForPage (0);
//			App.Screenshot ("On Page 0");
//		}

//		void AllElementExistForPage (int index)
//		{
//			var title = "Lorem ipsum dolor sit amet #" + index;

//			ScrollUpForQuery (q => q.Marked (title));

//			var buttons = new [] { 
//				"Insert Tab: " + index,
//				"Change title: " + index,
//				"Move Tab: " + index,
//				"Remove Tab: " + index,
//				"Reset all tabs: " + index,
//				"Next Page: " + index,
//			};

//			App.WaitForElement (q => q.Marked (title));

//			foreach (var button in buttons)
//				ScrollDownForQuery (q => q.Button (button));

//			ScrollUpForQuery (q => q.Marked (title));
//		}

//		void ScrollDownForQuery (Func<AppQuery, AppQuery> query)
//		{
//			var screenBounds = App.MainScreenBounds ();
//			App.DragFromToForElement (2, query, screenBounds.Width - 10, (2 / 3.0f) * screenBounds.Height, screenBounds.Width - 10, screenBounds.Height / 3.0f);
//		}

//		void ScrollUpForQuery (Func<AppQuery, AppQuery> query)
//		{
//			var screenBounds = App.MainScreenBounds ();
//			App.DragFromToForElement (2, query, screenBounds.Width - 10, screenBounds.Height / 3.0f, screenBounds.Width - 10,  (2 / 3.0f) * screenBounds.Height);
//		}

//		[Test]
//		[Description ("Insert tab - Landscape")]
//		public void TemplatedTabPageGalleryInsertTabLandscape () 
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TemplatedTabPageGalleryInsertTab ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Insert tab crash reproduction - Landscape")]
//		public void TemplatedTabPageGalleryInsertTabCrashLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TemplatedTabPageGalleryInsertTabCrash ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Change tab Title - Landscape")]
//		public void TemplatedTabPageGalleryChangeTitlesLandscape () 
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TemplatedTabPageGalleryChangeTitles ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Move tabs - Landscape")]
//		public void TemplatedTabPageGalleryMoveTabsLandscape () 
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TemplatedTabPageGalleryMoveTabs ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Remove tabs - Landscape")]
//		public void TemplatedTabPageGalleryRemoveTabLandscapes () 
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TemplatedTabPageGalleryRemoveTabs ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Add / remove tabs - Landscape")]
//		public void TemplatedTabPageGalleryAddRemoveTabsLandscape () 
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TemplatedTabPageGalleryAddRemoveTabs ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Reset tabs - Landscape")]
//		public void TemplatedTabPageGalleryResetAllTabsLandscape () 
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TemplatedTabPageGalleryResetAllTabs ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Go to next tabs - Landscape")]
//		public void TemplatedTabPageGalleryNextPageLandscape () 
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TemplatedTabPageGalleryNextPage ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

	}
}
