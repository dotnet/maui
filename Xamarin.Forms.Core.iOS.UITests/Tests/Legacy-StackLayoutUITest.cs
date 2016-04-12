using NUnit.Framework;
using Xamarin.UITest;
using System.Collections.Generic;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("StackLayout")]
	internal class StackLayoutGalleryTests : BaseTestFixture
	{
		// TODO
		// TODO: Port to new conventions

		public StackLayoutGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.StackLayoutGalleryLegacy);
		}

//		void AllElementsPresent ()
//		{
//			var buttons = new [] {
//				"Boring",
//				"Exciting!",
//				"Amazing!",
//				"Meh"
//			};

//			foreach (var button in buttons) {
//				App.WaitForElement (q => q.Button (button));
//			}

//			App.Screenshot ("All elements exist");
//		}

		[Test]
		[Description ("Check that each button is there and click them")]
		public void StackLayoutGalleryClickEachButton ()
		{
//			AllElementsPresent ();

//			App.Tap (q => q.Button ("Boring"));
//			App.WaitForElement (q => q.Button ("clicked1"), "Timeout : clicked1");

//			App.Tap (q => q.Button ("Exciting!"));
//			App.WaitForElement (q => q.Button ("clicked2"), "Timeout : clicked2");

//			App.Tap (q => q.Button ("Amazing!"));
//			App.WaitForElement (q => q.Button ("clicked3"), "Timeout : clicked3");

//			App.Tap (q => q.Button ("Meh"));
//			App.WaitForElement (q => q.Button ("clicked4"), "Timeout : clicked4");

//			App.Screenshot ("All buttons clicked");
		}

//		[Test]
//		[Description ("Check that each button is there and click them - Landscape")]
//		public void StackLayoutGalleryClickEachButtonLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			StackLayoutGalleryClickEachButton ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

	}
}
