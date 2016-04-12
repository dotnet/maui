using NUnit.Framework;
using Xamarin.UITest;
using System;
using System.Threading;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("Templated")]
	internal class BoundViewGalleryTests : BaseTestFixture
	{
		// TODO: Port to new conventions

		public BoundViewGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.BoundPageGalleryLegacy);
		}

		//void AllElementsPresent ()
		//{
		//	App.WaitForElement (q => q.Button ("Click Me!"));
		//	App.Screenshot ("All elements present");
		//}

		[Test]
		[Description ("Test bound view navigation")]
		public void BoundViewGalleryNavigateToAndBack ()
		{
			App.Screenshot ("At Gallery");

//			AllElementsPresent ();

//			App.Tap (q => q.Button ("Click Me!"));
//			App.WaitForElement (q => q.Marked ("Second Page"), "Timeout : Second Page");
//			App.Screenshot ("Navigation to next page successful");

//			App.Tap (PlatformQueries.Back);
//			App.WaitForElement (q => q.Button ("Click Me!"), "Timeout : Click Me!");
//			App.Screenshot ("Navigation back successful");
		}

//		[Test]
//		[Description ("Test button click")]
//		public void BoundViewGalleryClickButton ()
//		{
//			App.Tap (q => q.Button ("Click Me!"));
//			App.WaitForElement (q => q.Marked ("Second Page"), "Timeout : Second Page");
//		}


//		[Test]
//		[Description ("Verify all elements are preset - landscape")]
//		public void BoundViewGalleryAllElementsPresentLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			Thread.Sleep (1000);
//			App.Screenshot ("Rotated to Landscape");
//			AllElementsPresent ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Test bound view navigation- landscape")]
//		public void BoundViewGalleryNavigateToAndBackLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			Thread.Sleep (1000);
//			App.Screenshot ("Rotated to Landscape");
//			BoundViewGalleryNavigateToAndBack ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Test button click - landscape")]
//		public void BoundViewGalleryClickButtonLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			Thread.Sleep (1000);
//			App.Screenshot ("Rotated to Landscape");
//			BoundViewGalleryClickButton ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

	}
}
