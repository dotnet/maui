using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Threading;

using Xamarin.Forms.CustomAttributes;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("Editor")]
	internal class EditorGalleryTests : BaseTestFixture
	{
		// TODO: Port to new conventions

		public EditorGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.EditorGalleryLegacy);
		}

		[Test]
		[Category ("ManualReview")]
		[Description ("Try Default keyboard")]
		[UiTest (typeof (Editor), "Keyboard")]
		[UiTest (typeof (Keyboard), "Default")]
		public void EditorGalleryDefaultKeyboardGallery ()
		{
//			App.Tap (q => q.Marked ("Default Keyboard"));
//			App.WaitForElement (PlatformQueries.Editors, "Timeout : Editors");
//			App.Screenshot ("At Default Keyboard Gallery");

//			App.Tap (PlatformQueries.Editors);
//			// App.KeyboardIsPresent ();
//			App.Screenshot ("Keyboard showing");

//			var text = "This is some text that I am entering";
//			App.EnterText (PlatformQueries.Editors, text);
//			App.WaitForElement (PlatformQueries.EditorsWithText (text), "Timeout : Editor with Text " + text);
//			App.Screenshot ("Text Entered");

//			App.PressEnter ();
//			// App.KeyboardIsDismissed ();
//			App.Screenshot ("Pressed enter");
		}

//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Editor.Completed Fires")]
//		[UiTest (Test.Views.Editor)]
//		[UiTest (Test.Editor.Completed)]
//		public void EditorGalleryDefaultKeyboardGalleryCompletedFires ()
//		{
//			App.Tap (q => q.Marked ("Default Keyboard"));
//			App.WaitForElement (PlatformQueries.Editors, "Timeout : Editors");
//			App.Screenshot ("At Default Keyboard Gallery");

//			App.Tap (PlatformQueries.Editors);
//			// App.KeyboardIsPresent ();
//			App.Screenshot ("Keyboard showing");
//			App.EnterText (PlatformQueries.Editors, "First Line");
//			App.PressEnter ();

//			App.Screenshot ("Pressed enter");
//			App.EnterText (PlatformQueries.Editors, "Second Line");
//			App.Screenshot ("Entered second line");
//			App.Tap (PlatformQueries.LabelWithText("Nothing entered"));
//			App.Screenshot ("Keyboard dismissed");
////			App.Tap (q => q.Marked ("Done"));


//			App.WaitForNoElement (q => q.Marked ("Nothing entered"));
//			App.Screenshot ("Test complete");
////			App.WaitForElement (q => q.Marked ("Entered : First Line Second Line"));
//		}

//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Try Chat keyboard")]
//		[UiTest (Test.Views.Editor)]
//		[UiTest (Test.InputView.Keyboard)]
//		[UiTest (Test.Keyboard.Chat)]
//		public void EditorGalleryChatKeyboardGallery ()
//		{
//			App.Tap (q => q.Marked ("Chat Keyboard"));
//			App.WaitForElement (PlatformQueries.Editors, "Timeout : Editors");
//			App.Screenshot ("At Chat Keyboard Gallery");

//			App.Tap (PlatformQueries.Editors);
//			// App.KeyboardIsPresent ();
//			App.Screenshot ("Keyboard showing");

//			var text = "This is some text that I am entering";
//			App.EnterText (PlatformQueries.Editors, text);
//			App.WaitForElement (PlatformQueries.EditorsWithText (text), "Timeout : Editor with Text " + text);
//			App.Screenshot ("Text Entered");

//			App.PressEnter ();
//			// App.KeyboardIsDismissed ();
//			App.Screenshot ("Pressed enter");
//		}

//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Try Text keyboard")]
//		[UiTest (Test.Views.Editor)]
//		[UiTest (Test.InputView.Keyboard)]
//		[UiTest (Test.Keyboard.Text)]
//		public void EditorGalleryTextKeyboardGallery ()
//		{
//			App.Tap (q => q.Marked ("Text Keyboard"));
//			App.WaitForElement (PlatformQueries.Editors, "Timeout : Editors");
//			App.Screenshot ("At Text Keyboard Gallery");

//			App.Tap (PlatformQueries.Editors);
//			// App.KeyboardIsPresent ();
//			App.Screenshot ("Keyboard showing");

//			var text = "This is some text that I am entering";
//			App.EnterText (PlatformQueries.Editors, text);
//			App.WaitForElement (PlatformQueries.EditorsWithText (text), "Timeout : Editor with Text " + text);
//			App.Screenshot ("Text Entered");

//			App.PressEnter ();
//			// App.KeyboardIsDismissed ();
//			App.Screenshot ("Pressed enter");
//		}

//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Try Url keyboard")]
//		[UiTest (Test.Views.Editor)]
//		[UiTest (Test.InputView.Keyboard)]
//		[UiTest (Test.Keyboard.Url)]
//		public void EditorGalleryUrlKeyboardGallery ()
//		{
//			App.Tap (q => q.Marked ("Url Keyboard"));
//			App.WaitForElement (PlatformQueries.Editors, "Timeout : Editors");
//			App.Screenshot ("At Url Keyboard Gallery");

//			App.Tap (PlatformQueries.Editors);
//			// App.KeyboardIsPresent ();
//			App.Screenshot ("Keyboard showing");

//			var text = "https://www.xamarin.com";
//			App.EnterText (PlatformQueries.Editors, text);
//			App.WaitForElement (PlatformQueries.EditorsWithText (text), "Timeout : Editor with Text " + text);
//			App.Screenshot ("Text Entered");

//			App.PressEnter ();
//			// App.KeyboardIsDismissed ();
//			App.Screenshot ("Pressed enter");
//		}

//		[Test]
//		[Category ("ManualReview")]
//		[Description ("Try Numeric keyboard")]
//		[UiTest (Test.Views.Editor)]
//		[UiTest (Test.InputView.Keyboard)]
//		[UiTest (Test.Keyboard.Numeric)]
//		public void EditorGalleryNumericKeyboardGallery ()
//		{
//			App.Tap (q => q.Marked ("Numeric Keyboard"));
//			App.WaitForElement (PlatformQueries.Editors, "Timeout : Editors");
//			App.Screenshot ("At Numeric Keyboard Gallery");

//			App.Tap (PlatformQueries.Editors);
//			// App.KeyboardIsPresent ();
//			App.Screenshot ("Keyboard showing");

//			var text = "12345678910";
//			App.EnterText (PlatformQueries.Editors, text);
//			App.WaitForElement (PlatformQueries.EditorsWithText (text), "Timeout : Editor with Text " + text);
//			App.Screenshot ("Text Entered");
//		}

//		[Test]
//		[Description ("TextChanged event")]
//		[UiTest (Test.Views.Editor)]
//		[UiTest (Test.Editor.TextChanged)]
//		public void EditorGalleryDefaultKeyboardTextChanged ()
//		{
//			App.Tap (q => q.Marked ("Default Keyboard"));
//			App.WaitForElement (PlatformQueries.Editors, "Timeout : Editors");
//			App.Screenshot ("At Default Keyboard Gallery");

//			App.EnterText (PlatformQueries.Editors, "ABC");
//			App.Screenshot ("Entered three characters");
//			App.WaitForElement (PlatformQueries.LabelWithText ("xxx"));
//			var labelText = App.GetTextForQuery (PlatformQueries.LabelWithIndex (1));
//			Assert.AreEqual ("xxx", labelText);
//		}

//		[Test]
//		[Description ("TextChanged event - Issue #")]
//		[UiTest (Test.Views.Editor)]
//		[UiTest (Test.Editor.TextChanged)]
//		public void EditorGalleryChatKeyboardTextChanged ()
//		{
//			App.Tap (q => q.Marked ("Chat Keyboard"));
//			App.WaitForElement (PlatformQueries.Editors, "Timeout : Editors");
//			App.Screenshot ("At Chat Keyboard Gallery");

//			App.EnterText (PlatformQueries.Editors, "ABC");
//			App.Screenshot ("Entered three characters");
//			App.WaitForElement (PlatformQueries.LabelWithText ("xxx"));
//			var labelText = App.GetTextForQuery (PlatformQueries.LabelWithIndex (1));
//			Assert.AreEqual ("xxx", labelText);
//		}

//		[Test]
//		[Description ("TextChanged event")]
//		[UiTest (Test.Views.Editor)]
//		[UiTest (Test.Editor.TextChanged)]
//		public void EditorGalleryNumericKeyboardTextChanged ()
//		{
//			App.Tap (q => q.Marked ("Numeric Keyboard"));
//			App.WaitForElement (PlatformQueries.Editors, "Timeout : Editors");
//			App.Screenshot ("At Numeric Keyboard Gallery");

//			App.EnterText (PlatformQueries.Editors, "123");
//			App.Screenshot ("Entered three characters");
//			App.WaitForElement (PlatformQueries.LabelWithText ("xxx"));
//			var labelText = App.GetTextForQuery (PlatformQueries.LabelWithIndex (1));
//			Assert.AreEqual ("xxx", labelText);
//		}

//		[Test]
//		[Description ("TextChanged event")]
//		[UiTest (Test.Views.Editor)]
//		[UiTest (Test.Editor.TextChanged)]
//		public void EditorGalleryTextKeyboardTextChanged ()
//		{
//			App.Tap (q => q.Marked ("Text Keyboard"));
//			App.WaitForElement (PlatformQueries.Editors, "Timeout : Editors");
//			App.Screenshot ("At Text Keyboard Gallery");

//			App.EnterText (PlatformQueries.Editors, "ABC");
//			App.Screenshot ("Entered three characters");
//			App.WaitForElement (PlatformQueries.LabelWithText ("xxx"));
//			var labelText = App.GetTextForQuery (PlatformQueries.LabelWithIndex (1));
//			Assert.AreEqual ("xxx", labelText);
//		}

//		[Test]
//		[Description ("TextChanged event")]
//		[UiTest (Test.Views.Editor)]
//		[UiTest (Test.Editor.TextChanged)]
//		public void EditorGalleryUrlKeyboardTextChanged ()
//		{
//			App.Tap (q => q.Marked ("Url Keyboard"));
//			App.WaitForElement (PlatformQueries.Editors, "Timeout : Editors");
//			App.Screenshot ("At Url Keyboard Gallery");

//			App.EnterText (PlatformQueries.Editors, "ABC");
//			App.Screenshot ("Entered three characters");
//			App.WaitForElement (PlatformQueries.LabelWithText ("xxx"));
//			var labelText = App.GetTextForQuery (PlatformQueries.LabelWithIndex (1));
//			Assert.AreEqual ("xxx", labelText);
//		}
	}
}
