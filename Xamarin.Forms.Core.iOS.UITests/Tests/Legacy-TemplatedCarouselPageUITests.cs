using NUnit.Framework;
using Xamarin.UITest;
using System.Threading;
using Xamarin.UITest.Queries;
using System;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("Templated")]
	internal class TemplatedCarouselPageGalleryTests : BaseTestFixture
	{
		// TODO: Port to new conventions

		public TemplatedCarouselPageGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.TemplatedCarouselPageGalleryLegacy);
		}
		//AppRect scrollContainerBounds = new AppRect ();

//		void AllElementsPresent ()
//		{
//			AllElementExistForPage (0);

//			App.Screenshot ("All elements found on page 0");

//			SwipeLeft ();

//			App.Screenshot ("Swiped left");

//			AllElementExistForPage (1);

//			App.Screenshot ("All elements found on page 1");
//		}

		[Test]
		[Description ("Insert page")]
		public void TemplatedCarouselPageGalleryInsertPage ()
		{
//			AllElementsPresent ();

//			App.Tap (q => q.Button ("Insert Tab: 1"));

//			SwipeLeft ();

//			AllElementExistForPage (2);
//			App.Screenshot ("At page 2");

//			SwipeRight ();

//			AllElementExistForPage (1);
//			App.Screenshot ("At page 1");

//			SwipeRight ();

//			AllElementExistForPage (0);
//			App.Screenshot ("At page 0");
		}

//		[Test]
//		[Description ("Remove page")]
//		public void TemplatedCarouselPageGalleryRemovePage ()
//		{
//			AllElementsPresent ();

//			App.Tap (q => q.Button ("Remove Tab: 1"));

//			AllElementExistForPage (0);
//			App.Screenshot ("Removed page 1");
//		}

//		[Test]
//		[Description ("Insert / Remove page")]
//		public void TemplatedCarouselPageGalleryAddRemovePage ()
//		{
//			AllElementsPresent ();

//			App.Tap (q => q.Button ("Insert Tab: 1"));

//			SwipeLeft ();

//			AllElementExistForPage (2);
//			App.Screenshot ("At page 2");

//			App.Tap (q => q.Button ("Remove Tab: 2"));
//			AllElementExistForPage (0);
//			App.Screenshot ("Removed page 2");

//			SwipeLeft ();

//			App.Tap (q => q.Button ("Remove Tab: 1"));
//			AllElementExistForPage (0);
//			App.Screenshot ("Removed page 1");
//		}

//		[Test]
//		[Description ("Reset pages")]
//		public void TemplatedCarouselPageGalleryResetAllPages ()
//		{
//			AllElementsPresent ();

//			App.WaitForElement (q => q.Button ("Insert Tab: 1"));
//			App.Tap (q => q.Button ("Insert Tab: 1"));

//			SwipeLeft ();
//			AllElementExistForPage (2);
//			App.Tap (q => q.Button ("Insert Tab: 2"));

//			SwipeLeft ();
//			AllElementExistForPage (3);
//			App.Screenshot ("At page 3");

//			SwipeRight ();
//			App.Tap (q => q.Button ("Reset all tabs: 2"));

//			AllElementExistForPage (0);
//			App.Screenshot ("Pages reset");

//			SwipeLeft ();
//			AllElementExistForPage (1);
//			App.Screenshot ("On Page 1 again");

//			SwipeLeft ();
//			AllElementExistForPage (1);
//			App.Screenshot ("On Page 1 again");
//		}

//		[Test]
//		[Description ("Insert / go to next pages")]
//		public void TemplatedCarouselPageGalleryNextPage ()
//		{
//			TemplatedCarouselPageGalleryInsertPage ();

//			AppRect screenSize = App.MainScreenBounds ();
//			ScrollDownForQuery (q => q.Button ("Delayed reset: 0"), scrollContainerBounds);
//			App.Tap (q => q.Button ("Next Page: 0"));
//			AllElementExistForPage (1);
//			App.Screenshot ("At page 1");

//			ScrollDownForQuery (q => q.Button ("Delayed reset: 1"), scrollContainerBounds);
//			App.Tap (q => q.Button ("Next Page: 1"));
//			AllElementExistForPage (2);
//			App.Screenshot ("At page 2");

//			ScrollDownForQuery (q => q.Button ("Delayed reset: 2"), scrollContainerBounds);
//			App.Tap (q => q.Button ("Next Page: 2"));
//			AllElementExistForPage (0);
//			App.Screenshot ("At page 0");
//		}

//		[Test]
//		[Description ("Reproduction for a crash related to adding / reseting pages")]
//		public void TemplatedCarouselPageGalleryAddResetCrash ()
//		{
//			AllElementsPresent ();

//			SwipeRight ();

//			App.Tap (q => q.Button ("Insert Tab: 0"));
//			App.Tap (q => q.Button ("Insert Tab: 0"));
//			App.Screenshot ("Added two pages from Page 0");

//			SwipeLeft ();
//			App.Tap (q => q.Button ("Insert Tab: 3"));
//			App.Tap (q => q.Button ("Insert Tab: 3"));
//			App.Screenshot ("Added two pages from Page 3");

//			App.Tap (q => q.Button ("Reset all tabs: 3"));
//			AllElementExistForPage (0);
//			App.Screenshot ("Pages reset without crashing");
//		}

//		[Test]
//		[Description ("Reproduction for a crash related to adding / reseting pages")]
//		public void TemplatedCarouselPageGalleryAnotherAddResetCrash ()
//		{
//			AllElementsPresent ();

//			App.WaitForElement (q => q.Button ("Insert Tab: 1"));
//			App.Tap (q => q.Button ("Insert Tab: 1"));

//			SwipeLeft ();

//			AllElementExistForPage (2);
//			App.Screenshot ("At page 2");

//			App.Tap (q => q.Button ("Reset all tabs: 2"));
//			App.WaitForElement (q => q.Marked ("Insert Tab: 0"));
//			AllElementExistForPage (0);
//		}

//		[Test]
//		[Description ("Delayed reset of all content")]
//		public void TemplatedCarouselPageGalleryDelayResetAllElements ()
//		{
//			AllElementsPresent ();

//			App.ScrollDownForElement (q => q.Button ("Delayed reset: 1"), 2);
//			App.Tap (q => q.Button ("Delayed reset: 1"));

//			App.WaitForNoElement (q => q.Marked ("Lorem ipsum dolor sit amet #1"));
//			App.WaitForElement (q => q.Marked ("Insert Tab: 0"));
//			AllElementExistForPage (0);
//		}

//		[Test]
//		[Description ("Reproduction for a crash related to removing the first page")]
//		public void TemplatedCarouselPageGalleryRemoveFirstPageAndResetCrash ()
//		{
//			AllElementsPresent ();

//			SwipeRight ();
//			AllElementExistForPage (0);

//			App.Tap (q => q.Marked ("Remove Tab: 0"));
//			App.Screenshot ("Remove first page");

//			AllElementExistForPage (1);
//			App.Tap (q => q.Marked ("Reset all tabs: 1"));

//			AllElementExistForPage (0);
//			App.Screenshot ("Reset all pages");
//		}

//		void SwipeLeft ()
//		{

//			AppRect swipeLabelBounds = App.Query (q => q.Marked ("Swipe Here"))[0].Rect;
//			// Account for padded scrollview implementations on the different platforms
//			App.DragFromTo (
//				scrollContainerBounds.X + scrollContainerBounds.Width - PlatformValues.OffsetForScrollView, 
//				swipeLabelBounds.CenterY, 
//				scrollContainerBounds.X + PlatformValues.OffsetForScrollView, 
//				swipeLabelBounds.CenterY
//			);
//			Thread.Sleep (2000);
//		}

//		void SwipeRight ()
//		{

//			AppRect swipeLabelBounds = App.Query (q => q.Marked ("Swipe Here"))[0].Rect;
//			// Account for padded scrollview implementations on the different platforms
//			App.DragFromTo (
//				scrollContainerBounds.X + PlatformValues.OffsetForScrollView, 
//				swipeLabelBounds.CenterY, 
//				scrollContainerBounds.X + scrollContainerBounds.Width - PlatformValues.OffsetForScrollView, 
//				swipeLabelBounds.CenterY
//			);
//			Thread.Sleep (2000);
//		}

//		void ScrollDownForQuery (Func<AppQuery, AppQuery> query, AppRect scrollContainer)
//		{
//			AppRect screenSize = App.MainScreenBounds ();
//			float swipeY = scrollContainer.X + 5;
//			App.DragFromToForElement (5, query, swipeY, (2 / 3.0f) * screenSize.Height, swipeY, screenSize.Height / 3.0f);
//		}

//		void ScrollUpForQuery (Func<AppQuery, AppQuery> query, AppRect scrollContainer)
//		{
//			AppRect screenSize = App.MainScreenBounds ();
//			float swipeY = scrollContainer.X + 5;
//			App.DragFromToForElement (2, query, swipeY, screenSize.Height / 3.0f, swipeY,  (2 / 3.0f) * screenSize.Height);
//		}


//		void AllElementExistForPage (int index)
//		{
//			var title = "Lorem ipsum dolor sit amet #" + index;
//			// Wait for element to load before querying its parent (problem on iOS)
//			if (App.Query (q => q.Marked (title)).Length < 1)
//				App.ScrollUpForElement (q => q.Marked (title), 3);
//			App.WaitForElement (q => q.Marked (title));
//			scrollContainerBounds = App.Query (q => q.Marked (title).Parent ())[2].Rect;

//			ScrollUpForQuery (q => q.Marked (title), scrollContainerBounds);

//			var buttons = new [] { 
//				"Insert Tab: " + index,
//				"Change title: " + index,
//				"Remove Tab: " + index,
//				"Reset all tabs: " + index,
//				"Next Page: " + index,
//				"Delayed reset: " + index,
//			};

//			App.WaitForElement (q => q.Marked (title), "Timeout: " + title);

//			foreach (var button in buttons) {
//				if (App.Query (q => q.Button (button)).Length < 1)
//					ScrollDownForQuery (q=> q.Button (button), scrollContainerBounds);
//				App.WaitForElement (q => q.Button (button));
//			}
//			ScrollUpForQuery (q => q.Marked (title), scrollContainerBounds);
//		}


//		[Test]
//		[Description ("Insert page - Landscape")]
//		public void TemplatedCarouselPageGalleryInsertPageLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TemplatedCarouselPageGalleryInsertPage ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Remove page - Landscape")]
//		public void TemplatedCarouselPageGalleryRemovePageLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TemplatedCarouselPageGalleryRemovePage ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Insert / Remove page - Landscape")]
//		public void TemplatedCarouselPageGalleryAddRemovePageLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TemplatedCarouselPageGalleryAddRemovePage ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Reset pages - Landscape")]
//		public void TemplatedCarouselPageGalleryResetAllPagesLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TemplatedCarouselPageGalleryResetAllPages ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Insert / go to next pages - Landscape")]
//		public void TemplatedCarouselPageGalleryNextPageLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TemplatedCarouselPageGalleryNextPage ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Reproduction for a crash related to adding / reseting pages - Landscape")]
//		public void TemplatedCarouselPageGalleryAddResetCrashLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TemplatedCarouselPageGalleryAddResetCrash ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Reproduction for a crash related to adding / reseting pages - Landscape")]
//		public void TemplatedCarouselPageGalleryAnotherAddResetCrashLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TemplatedCarouselPageGalleryAnotherAddResetCrash ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Delayed reset of all content - Landscape")]
//		public void TemplatedCarouselPageGalleryDelayResetAllElementsLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TemplatedCarouselPageGalleryDelayResetAllElements ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Reproduction for a crash related to removing the first page - Landscape")]
//		public void TemplatedCarouselPageGalleryRemoveFirstPageAndResetCrashLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TemplatedCarouselPageGalleryRemoveFirstPageAndResetCrash ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

	}
}
