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
	[Category ("Switch")]
	internal class SwitchGalleryTests : BaseTestFixture
	{
		// TODO: Checking enabled / disabled states
		// TODO: Port to new conventions

		public SwitchGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.SwitchGalleryLegacy);
		}

		[Test]
		[Description ("Check all elements exist")]
		public void SwitchGalleryAllElementsPresent ()
		{
//			var label = App.Query (PlatformQueries.LabelWithText("Test Label"));
//			Assert.AreEqual (1, label.Length);

//			var switches = App.Query (q => q.Raw ("Switch"));
//			Assert.AreEqual (3, switches.Length);

//			var steppers = App.Query (PlatformQueries.Steppers);
//			Assert.AreEqual (1, steppers.Length);

//			App.Screenshot ("All elements exist");
		}

//		[Test]
//		[Description ("Check that events fire")]
////		[Category ("Single")]
//		public void SwitchGalleryEventTest ()
//		{
//			App.Tap (PlatformQueries.SwitchWithIndex (0));
//			App.Screenshot ("Toggled normal switch");
//			App.WaitForElement (PlatformQueries.LabelWithText ("Toggled normal switch"));
//			var labelText = App.GetTextForQuery (PlatformQueries.LabelWithText ("Toggled normal switch"));
//			Assert.AreEqual ("Toggled normal switch", labelText);

//			App.Tap (PlatformQueries.SwitchWithIndex (1));
//			App.Screenshot ("Tried to toggle disabled switch");
//			App.WaitForElement (PlatformQueries.LabelWithText ("Toggled normal switch"));
//			labelText = App.GetTextForQuery (PlatformQueries.LabelWithText ("Toggled normal switch"));
//			Assert.AreEqual ("Toggled normal switch", labelText);

//			App.Tap (PlatformQueries.SwitchWithIndex (2));
//			App.Screenshot ("Toggled transparent switch");
//			App.WaitForElement (PlatformQueries.LabelWithText ("Toggled transparent switch"));
//			labelText = App.GetTextForQuery (PlatformQueries.LabelWithText ("Toggled transparent switch"));
//			Assert.AreEqual ("Toggled transparent switch", labelText);

//			for (int i = 1; i <= 5; i++) {
//				App.Tap (PlatformQueries.Steppers);
//				App.Screenshot (string.Format ("Tapped stepper {0} times", i));
//				App.WaitForElement (PlatformQueries.LabelWithText (i.ToString ()));
//				labelText = App.GetTextForQuery (PlatformQueries.LabelWithText (i.ToString ()));
//				Assert.AreEqual (i.ToString (), labelText);
//			}
//		}

//		[Test]
//		[Description ("Check all elements exist - Landscape")]
//		public void SwitchGalleryAllElementsPresetLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			SwitchGalleryAllElementsPresent ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Check that events fire - Landscape")]
////		[Category ("Single")]
//		public void SwitchGalleryEventTestLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			SwitchGalleryEventTest ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}
	
	}
}
