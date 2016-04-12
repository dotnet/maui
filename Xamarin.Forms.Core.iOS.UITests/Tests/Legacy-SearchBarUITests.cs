using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Threading;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("SearchBar")]
	internal class SearchBarGalleryTests : BaseTestFixture
	{
		// TODO: Enter text and try searching
		// TODO: Port to new conventions

		public SearchBarGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.SearchBarGalleryLegacy);
		}
	
		[Test]
		[Category ("ManualReview")]
		[Description ("Enter query into each SearchBar")]
		public void SearchBarGalleryEnterSearchText ()
		{
//			SearchBarGalleryAllElementsPresent ();
//			for (var i = 0; i < 3; i++) {
//				App.ScrollDownForElement (PlatformQueries.SearchBarWithIndex (i), 5);
//				App.Tap (PlatformQueries.SearchBarWithIndex (i));
//				App.EnterText (PlatformQueries.SearchBarWithIndex (i), "Search: " + i);
//				App.Screenshot ("Keyboard should be shown");
//				App.PressEnter ();
//				App.WaitForElement (q => q.Marked ("Search: " + i));
//				App.Screenshot (string.Format("Keyboard should be dismissed - Label should have changed to 'Search: {0}'", i));
//			}

//			App.Tap (q => q.Button ("More SearchBars"));
//			App.WaitForElement (q => q.Marked ("Search Query 2"));

//			SearchBarGalleryAllElementsPresentPageTwo ();

//			// Disabled keyboard
//			App.Tap (PlatformQueries.SearchBarWithIndex (0));
//			App.Screenshot ("Should not see keyboard for disabled SearchBar");

//			App.Tap (PlatformQueries.SearchBarWithIndex (1));
//			App.Screenshot ("Should not see keyboard for disabled SearchBar");
//			App.EnterText (PlatformQueries.SearchBarWithIndex (1), "Entered transparent");
//			App.PressEnter ();
//			App.WaitForElement (q => q.Marked ("Entered transparent"));
//			App.Screenshot ("Entered query for tranparent SearchBar");
		}
			
//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Enable disable keyboard, Issues #1182, #1237")]
//		public void SearchBarGalleryEnableDisableSearchBar () 
//		{
//			App.Tap (q => q.Button ("More SearchBars"));
//			App.WaitForElement (q => q.Marked ("Search Query 2"));

//			SearchBarGalleryAllElementsPresentPageTwo ();

//			App.Tap (PlatformQueries.SearchBarWithIndex (0));
//			App.Screenshot ("SearchBar should not be focused, keyboard should not be shown");

//			App.Tap (q => q.Button ("Toggle enabled"));
//			App.Tap (PlatformQueries.SearchBarWithIndex (0));
//			App.EnterText (PlatformQueries.SearchBarWithIndex (0), "Now Enabled");
//			App.PressEnter ();
//			App.ScrollDownForElement (q => q.Marked ("Now Enabled"), 2);
//			App.Screenshot ("Enabled and abled to query");

//			App.Tap (q => q.Button ("Toggle enabled"));
//			App.Screenshot ("Disabled again");

//			App.ScrollUpForElement (PlatformQueries.SearchBarWithIndex (0), 2);
//			App.Tap (PlatformQueries.SearchBarWithIndex (0));
//			App.Screenshot ("SearchBar should not be focused, keyboard should not be shown after diabling once again");
//		}

//		[Test]
////		[Category ("Single")]
//		[Description ("Test the TextChanged event")]
//		public void SearchBarGalleryTextChangedEventTest ()
//		{
//			SearchBarGalleryAllElementsPresent ();

//			App.EnterText (PlatformQueries.SearchBarWithIndex (0), "A");
//			App.Screenshot ("Entered three characters in noPlaceholder search bar");
//			var labelText = App.GetTextForQuery (PlatformQueries.LabelWithText ("1"));
//			Assert.AreEqual ("1", labelText);

//			App.EnterText (PlatformQueries.SearchBarWithIndex (1), "B");
//			App.Screenshot ("Entered three characters in normal search bar");
//			labelText = App.GetTextForQuery (PlatformQueries.LabelWithText ("2"));
//			Assert.AreEqual ("2", labelText);

//			App.EnterText (PlatformQueries.SearchBarWithIndex (2), "C");
//			App.Screenshot ("Entered three characters in activation search bar");
//			labelText = App.GetTextForQuery (PlatformQueries.LabelWithText ("3"));
//			Assert.AreEqual ("3", labelText);

//		}

//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Enable disable keyboard, Issues #1182, #1237 - landscape")]
//		public void SearchBarGalleryEnableDisableSearchBarLandscape () 
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			SearchBarGalleryEnableDisableSearchBar ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Enter query into each SearchBar - Landscape")]
//		public void SearchBarGalleryEnterSearchTextLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			SearchBarGalleryEnterSearchText ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		void SearchBarGalleryAllElementsPresent ()
//		{
//			var searchBars = App.Query (PlatformQueries.SearchBars);
//			Assert.AreEqual (3, searchBars.Length);

//			App.ScrollDownForElement (q => q.Marked ("Search Query"), 5);

//			App.ScrollUp ();
//			App.Screenshot ("All SearchBars present");
//		}



//		void SearchBarGalleryAllElementsPresentPageTwo ()
//		{
//			App.ScrollDownForElement (q => q.Marked ("Search Query 2"), 5);
//			App.ScrollDownForElement (q => q.Button ("Toggle enabled"), 5);

//			App.ScrollUp ();
//			App.Screenshot ("All SearchBars present - Page 2");
//		}

	}
}
