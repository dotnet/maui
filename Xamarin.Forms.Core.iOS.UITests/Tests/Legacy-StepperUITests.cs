using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using System.Diagnostics;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("Stepper")]
	internal class StepperGalleryTests : BaseTestFixture
	{
		// TODO: Checking enabled / disabled states
		// TODO: Port to new conventions

		public StepperGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.StepperGalleryLegacy);
		}

		[Test]
		[Description ("Check all elements exist")]
		public void StepperGalleryAllElementsPresent ()
		{
//			App.WaitForElement (PlatformQueries.LabelWithText ("0"));
//			var labels = App.Query (PlatformQueries.LabelWithText ("0"));
//			Assert.AreEqual (2, labels.Length);

//			var steppers = App.Query (PlatformQueries.Steppers);
//			Assert.AreEqual (2, steppers.Length);

//			App.Screenshot ("All elements exist");
		}

//		[Test]
//		[Description ("Check that value changed event fires")]
//		public void StepperGalleryValueChangedEventTest ()
//		{
//			StepperGalleryAllElementsPresent ();
//			var labelText = "";
//			for (int i = 1; i <= 5; i++) {
//				App.Tap (PlatformQueries.StepperWithIndex (0));
//				App.Screenshot (string.Format ("Tapped first stepper {0} times", i));
//				App.WaitForElement (PlatformQueries.LabelWithText ((i*10).ToString ()));
//				labelText = App.GetTextForQuery (PlatformQueries.LabelWithText ((i*10).ToString ()));
//				Assert.AreEqual ((i*10).ToString (), labelText);
//			}

//			for (int i = 1; i <= 5; i++) {
//				App.Tap (PlatformQueries.StepperWithIndex (1));
//				App.Screenshot (string.Format ("Tapped second stepper {0} times", i));
//				App.WaitForElement (PlatformQueries.LabelWithText ((i*.05).ToString ()));
//				labelText = App.GetTextForQuery (PlatformQueries.LabelWithText ((i*.05).ToString ()));
//				Assert.AreEqual ((i*.05).ToString (), labelText);
//			}
//		}

//		[Test]
//		[Description ("Check all elements exist - Landscape")]
//		public void StepperGalleryAllElementsPresetLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			StepperGalleryAllElementsPresent ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Check that events fire - Landscape")]
////		[Category ("Single")]
//		public void StepperGalleryEventTestLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			StepperGalleryValueChangedEventTest ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}
	
	}
}
