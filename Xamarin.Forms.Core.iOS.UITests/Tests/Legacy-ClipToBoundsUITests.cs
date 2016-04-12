using System;
using System.Runtime;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Shared;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("AbsoluteLayout")]
	internal class ClipToBoundsGalleryTests : BaseTestFixture
	{
		// TODO detect size before and after clip
		// TODO: Port to new conventions

		public ClipToBoundsGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.ClipToBoundsGalleryLegacy);
		}
		[Test]
		[Description ("Check all elements exist")]
		public void ClipToBoundsGalleryAllElementsExist ()
		{
//			App.WaitForElement (q => q.Button ("Clip"), "Timeout : Clip");
//			var boxes = App.Query (PlatformQueries.BoxRendererQuery);
//			Assert.AreEqual (2, boxes.Length);
//			App.Screenshot ("2 boxes exist");


		}	

//		[Test]
//		[Description ("Clip boxes")]
//		public void ClipToBoundsGalleryClipElements ()
//		{
//			App.Tap (q => q.Button ("Clip"));
//			App.Screenshot ("Clip elements");
//		}

//		[Test]
//		[Description ("Check all elements exist - landscape")]
//		public void ClipToBoundsGalleryAllElementsExistLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			ClipToBoundsGalleryAllElementsExist ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}	

//		[Test]
//		[Description ("Clip boxes - landscape")]
//		public void ClipToBoundsGalleryClipElementsLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			ClipToBoundsGalleryClipElements ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
	//	}

	}
}

