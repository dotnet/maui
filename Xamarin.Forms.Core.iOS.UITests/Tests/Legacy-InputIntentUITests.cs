using System;
using System.Collections.Generic;
using System.Threading;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("Entry")]
	internal class InputIntentGalleryTests : BaseTestFixture
	{
		// TODO: Detect keyboard types, fix scroll coordinates
		// TODO: Port to new conventions

		public InputIntentGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.InputIntentGalleryLegacy);
		}
		
		[Test]
		[Description ("All entries are shown")]
		public void InputIntentGalleryAllElementsExist ()
		{
//			var inputs = new [] {
//				"Default",
//				"Email Input",
//				"Text Input",
//				"Url Input",
//				"Numeric Input",
//				"Telephone Input", 
//				"Chat Input",
//				"Custom Entry"
//			};

//			foreach (var input in inputs)
//				App.ScrollDownForElement (PlatformQueries.EntryWithPlaceholder (input), 5);

//			App.Screenshot ("All Entries exist");
		}

//		[Test]
//		[Description ("Tap each entry and dismiss keyboard by tapping outside of keyboard")]
//		public void InputIntentGalleryTapEachEntry ()
//		{
//			AppRect screenSize = App.MainScreenBounds ();
//			var numberOfEntries = App.Query (PlatformQueries.Entrys).Length;
//			App.Screenshot ("Tap each entry");

//			var inputs = new List<string> () {
//				"Default",
//				"Email Input",
//				"Text Input",
//				"Url Input",
//				"Numeric Input",
//				"Telephone Input", 
//				"Chat Input",
//				"Custom Entry"
//			};

//			foreach (var input in inputs) {
//				App.ScrollDownForElement (PlatformQueries.EntryWithPlaceholder (input), 5);
//				App.Tap (PlatformQueries.EntryWithPlaceholder (input));
//				App.Screenshot ("Input Tapped: " + input);
//				App.TapCoordinates (5, screenSize.Height / 3);
//				App.Screenshot ("Clicked main screen, keyboard should be dismissed");
//			}
				
//			App.ScrollUpForElement (q => q.Marked ("Custom Focused"), 5);

//			App.Screenshot ("Label should now say 'Custom Focused'");
//		}

//		[Test]
//		[Description ("Enter text in each entry")]
//		public void InputIntentGalleryEnterTextInEachEnry ()
//		{

//			AppRect screenSize = App.MainScreenBounds ();

//			var inputs = new Dictionary<string, string> () {
//				{ "Default", "abc Quick weee!" },
//				{ "Email Input", "s@test.com" },
//				{ "Text Input", "Hi, I am text!" },
//				{ "Url Input", "https://www.xamarin.com/" },
//				{ "Numeric Input", "12345678910" }, 
//				{ "Telephone Input", "0000001234" }, 
//				{ "Chat Input", "Sorry, I wasn\'t paying attention." }, 
//				{ "Custom Entry", "I should be custom" }
//			};

//			App.Screenshot ("Enter text in each input");

//			foreach (var input in inputs) {
//				App.ScrollDownForElement (PlatformQueries.EntryWithPlaceholder (input.Key), 5);
//				App.Tap (PlatformQueries.EntryWithPlaceholder (input.Key));
//				App.EnterText (PlatformQueries.EntryWithPlaceholder (input.Key), input.Value);
//				App.Screenshot ("Text entered");
//				App.TapCoordinates (5, screenSize.Height / 3);
//				App.Screenshot ("Clicked main screen, keyboard should be dismissed");
//			}
				
//		}

//		[Test]
//		[Description ("Open keyboard and navigate back without dismissing")]
//		public void InputIntentGalleryNavigateBackWithoutDismissingKeyboard ()
//		{
//			// Should catch any regression of Issue #638, #928
//			var inputs = new List<string> () {
//				"Default",
//				"Email Input",
//				"Text Input",
//				"Url Input",
//				"Numeric Input",
//				"Telephone Input", 
//				"Chat Input",
//				"Custom Entry"
//			};

//			foreach (string input in inputs) {
//				App.ScrollDownForElement (PlatformQueries.EntryWithPlaceholder (input), 5);
//				App.Tap (PlatformQueries.EntryWithPlaceholder (input));
//				App.Screenshot ("Input Tapped");

//				App.Tap (PlatformQueries.Back);
//				App.Screenshot ("Back at Control Gallery");
//				App.ScrollDownForElement (q => q.Button ("InputIntent"), 2);
//				App.Tap (q => q.Button ("InputIntent"));
//			}
//		}

//		[Test]
//		[Description ("All entries are shown - landscape")]
//		public void InputIntentGalleryAllElementsExistLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			InputIntentGalleryAllElementsExist ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to portrait");
//		}

//		[Test]
//		[Description ("Tap each entry and dismiss keyboard by tapping outside of keyboard - landscape")]
//		public void InputIntentGalleryTapEachEntryLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			InputIntentGalleryTapEachEntry ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to portrait");
//		}

//		[Test]
//		[Description ("Enter text in each entry")]
//		public void InputIntentGalleryEnterTextInEachEnryLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			InputIntentGalleryEnterTextInEachEnry ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to portrait");
//		}

//		[Test]
//		[Description ("Open keyboard and navigate back without dismissing")]
//		public void InputIntentGalleryNavigateBackWithoutDismissingKeyboardLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			InputIntentGalleryNavigateBackWithoutDismissingKeyboard ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to portrait");
//		}

	}
}


