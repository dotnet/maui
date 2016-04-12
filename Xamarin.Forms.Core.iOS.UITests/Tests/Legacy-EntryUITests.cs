using System;
using NUnit.Framework;
using System.Collections.Generic;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("Entry")]
	internal class EntryGalleryTests : BaseTestFixture
	{
		// TODO: Get Toggle color tests for both iOS and Android, Keyboard dismisses for Enter
		// TODO: Port to new conventions

		public EntryGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.EntryGalleryLegacy);
		}
		[Test]
		[Description ("All Entry Gallery elements are present")]
		public void EntryGalleryAllElementsPresent ()
		{

//			var entryElements = new [] {
//				q => q.Marked ("Enter something in Normal"),
//				q => q.Marked ("No typing has happened in Normal yet"),
//				PlatformQueries.EntryWithPlaceholder ("Normal"),
//				PlatformQueries.EntryWithPlaceholder ("Password"),
//				PlatformQueries.EntryWithPlaceholder ("Numeric Password"),
//				q => q.Marked ("Focus an Entry"),
//				PlatformQueries.EntryWithPlaceholder ("Disabled"),
//				PlatformQueries.EntryWithPlaceholder ("Activation"),
//				PlatformQueries.EntryWithPlaceholder ("Transparent"),
//				PlatformQueries.EntryWithPlaceholder ("Keyboard.Default")
//			};

//			foreach (var entry in entryElements)
//				App.ScrollDownForElement (entry, 10);

//			var buttons = new [] { 
//				"Toggle Text Color",
//				"Toggle Secure",
//				"Change Placeholder",
//				"Focus First"
//			};

//			foreach (var button in buttons)
//				App.ScrollDownForElement (q => q.Button (button), 10);

//			App.Screenshot ("All elements present");
		}

//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Check that the keyboard shows for each Entry")]
//		public void EntryGalleryKeyboardDisplays ()
//		{
//			AppRect windowBounds = App.MainScreenBounds ();

//			var placeHolders = new [] { 
//				"Normal",
//				"Password",
//				"Numeric Password",
//				"Disabled",
//				"Activation",
//				"Transparent",
//				"Keyboard.Default"
//			};

//			foreach (var placeholder in placeHolders) {
//				App.ScrollDownForElement (PlatformQueries.EntryWithPlaceholder (placeholder), 3);
//				if (placeholder != "Disabled") {
//					App.Tap (PlatformQueries.EntryWithPlaceholder (placeholder));
//					//App.KeyboardIsPresent ();
//					App.Screenshot ("Keyboard shown: " + placeholder);
//					// Tap empty part of screen
//					App.TapCoordinates (10, windowBounds.Height / 3);
//				} else { // Disabled entry should not show keyboard 
//					App.Tap (PlatformQueries.EntryWithPlaceholder (placeholder));
//					//App.KeyboardIsDismissed ();
//					App.Screenshot ("Keyboard not shown: " + placeholder);
//				}
//			}

//			App.Screenshot ("Keyboard should be dismissed");
//		}

//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Enter text in each entry")]
//		public void EntryGalleryEnterTextInEachFieldAndTapScreenDismiss ()
//		{
//			var entries = new [] { 
//				new { Placeholder = "Normal", LabelValue = "Normal Focused" },
//				new { Placeholder = "Password", LabelValue = "Password Focused" },
//				new { Placeholder = "Numeric Password", LabelValue = "Numeric Password Focused" },
//				new { Placeholder = "Disabled", LabelValue = "Disabled Focused" },
//				new { Placeholder = "Activation", LabelValue = "Activation Focused" },
//				new { Placeholder = "Transparent", LabelValue = "Transparent Focused" },
//				new { Placeholder = "Keyboard.Default", LabelValue = "Keyboard.Default Focused" },
//			};

//			AppRect windowBounds = App.MainScreenBounds ();
//			int helloNum = 0;

//			foreach (var entry in entries) {
//				App.ScrollDownForElement (PlatformQueries.EntryWithPlaceholder (entry.Placeholder), 3);
//				if (entry.Placeholder == "Disabled") {
//					App.Tap (PlatformQueries.EntryWithPlaceholder (entry.Placeholder));
//					// App.KeyboardIsDismissed ();
//				} else if (entry.Placeholder == "Numeric Password") {
//					App.Tap (PlatformQueries.EntryWithPlaceholder (entry.Placeholder));
//					// App.KeyboardIsPresent ();
//					App.EnterText (PlatformQueries.EntryWithPlaceholder (entry.Placeholder), "167728");
//					App.TapCoordinates (10, windowBounds.Height / 3);
//				} else {
//					App.Tap (PlatformQueries.EntryWithPlaceholder (entry.Placeholder));
//					// App.KeyboardIsPresent ();
//					App.EnterText (PlatformQueries.EntryWithPlaceholder (entry.Placeholder), "Hello " + helloNum);
//					App.TapCoordinates (10, windowBounds.Height / 3);
//					helloNum++;
//				} 
//			}

//			App.Screenshot ("Entered text in each entry, password should be hidden");
//			App.Tap (q => q.Button ("Toggle Secure"));
							
//			App.ScrollUpForElement (q => q.Marked ("Hello 0"), 5);
//			App.ScrollDownForElement (q => q.Marked ("Hello 1"), 5);
//			App.ScrollDownForElement (q => q.Marked ("167728"), 5);
//			App.ScrollDownForElement (q => q.Marked ("Hello 3"), 5);
//			App.ScrollDownForElement (q => q.Marked ("Hello 4"), 5);
//			App.ScrollDownForElement (q => q.Marked ("Hello 5"), 5);

//			App.Screenshot ("Entered text in each entry, password should be shown");
//		}

////		[Test]
////		public void ToggleTextColor ()
////		{
////			AllElementsPresent ();
////
////			var text = "hello";
////			App.EnterText (PlatformQueries.EntryWithPlaceholder ("Normal"), text);
////			var initialTextColor = App.Query (q => q.Raw (string.Format ("{0} {1}:'{2}', :getCurrentTextColor", PlatformStrings.Entry, PlatformStrings.Text, text)));
////			App.Tap (q => q.Button ("Toggle Text Color"));
////			var secondTextColor = App.Query (q => q.Raw (string.Format ("{0} {1}:'{2}', :getCurrentTextColor", PlatformStrings.Entry, PlatformStrings.Text, text)));
////			Assert.AreNotEqual (initialTextColor, secondTextColor);
////		} 

//		[Test]
//		[Description ("Change Placeholder in each entry")]
//		public void EntryGalleryChangePlaceholder ()
//		{
//			for (var i = 1; i <= 5; i++) {
//				App.ScrollDownForElement (q => q.Button ("Change Placeholder"), 5);
//				App.Tap (q => q.Button ("Change Placeholder"));
//				App.ScrollUpForElement (PlatformQueries.EntryWithPlaceholder ("Placeholder " + i), 5);
//			}

//			App.Screenshot ("Changed placeholder 5 times");
//		}

//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Update Keyboard Type - #1307")]
//		public void EntryGalleryChangeKeyboardType ()
//		{
//			App.ScrollDownForElement (PlatformQueries.EntryWithPlaceholder ("Keyboard.Default"), 5);
//			App.Tap (PlatformQueries.EntryWithPlaceholder ("Keyboard.Default"));
//			App.Screenshot ("I should see the default keyboard");
//			App.PressEnter ();
//			App.Screenshot ("Keyboard should be dismissed");
//			App.Screenshot ("Manually check that keyboard type is switched");
////			App.WaitForElement (PlatformQueries.EntryWithPlaceholder ("Keyboard.Numeric"));
////			App.Tap (PlatformQueries.EntryWithPlaceholder ("Keyboard.Numeric"));
//			App.Screenshot ("I should see the numeric keyboard");
//		}

//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Completed handler firing, keyboard dismissing - #1308")]
//		public void EntryGalleryCheckCompletedKeyboardDismissed ()
//		{
//			App.WaitForElement (q => q.Marked ("Enter something in Normal"));
//			App.ScrollDownForElement (PlatformQueries.EntryWithPlaceholder ("Normal"), 5);
//			App.Tap (PlatformQueries.EntryWithPlaceholder ("Normal"));
//			App.Screenshot ("Tapped Normal");
//			App.EnterText (PlatformQueries.EntryWithPlaceholder ("Normal"), "It has been entered");
//			App.Screenshot ("The keyboard should be shown");
//			// App.PressEnter ();
//			App.Screenshot ("Keyboard should be dismissed - Check manually");
//			//App.WaitForNoElement (q => q.Marked ("Enter something in Normal"));
//			//App.ScrollUpForElement (q => q.Marked ("It has been entered"), 5);
//			App.Screenshot ("Text should have changed");
//		}

//		[Test]
//		[Description ("Test the TextChanged event")]
//		public void EntryGalleryTextChangedEventTest ()
//		{
//			App.Screenshot ("Waiting for entry gallery");
//			App.WaitForElement (q => q.Marked ("No typing has happened in Normal yet"));
//			App.Screenshot ("Entering text in Normal");
//			App.EnterText (PlatformQueries.EntryWithPlaceholder ("Normal"), "a");
//			App.WaitForElement (q => q.Marked ("You typed in normal"));
//			App.Screenshot ("Text entered, TextChanged event should have fired");
//		}
			

//		[Test]
//		[Description ("All Entry Gallery elements are present - landscape")]
//		public void EntryGalleryAllElementsPresentLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			EntryGalleryAllElementsPresent ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}
			
//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Check that the keyboard shows for each Entry - landscape")]
//		public void EntryGalleryKeyboardDisplaysLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			EntryGalleryKeyboardDisplays ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Enter text in each entry - landscape")]
//		public void EntryGalleryEnterTextInEachFieldAndTapScreenDismissLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			EntryGalleryEnterTextInEachFieldAndTapScreenDismiss ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Change Placeholder in each entry - landscape")]
//		public void EntryGalleryChangePlaceholderLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			EntryGalleryChangePlaceholder ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Update Keyboard Type - #1307 - Landscape")]
//		public void EntryGalleryChangeKeyboardTypeLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			EntryGalleryChangeKeyboardType ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Completed handler firing, keyboard dismissing - #1308 - Landscape")]
//		public void EntryGalleryCheckCompletedKeyboardDismissedLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			EntryGalleryCheckCompletedKeyboardDismissed ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Test the TextChanged event - Landscape")]
//		public void EntryGalleryTextChangedEventTestLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			EntryGalleryTextChangedEventTest ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

	}
}
