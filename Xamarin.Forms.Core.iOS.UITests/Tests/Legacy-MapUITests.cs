using NUnit.Framework;
using Xamarin.UITest;
using System.Diagnostics;
using System.Threading;
using Xamarin.UITest.Queries;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("Map")]
	internal class MapGalleryTests : BaseTestFixture
	{
		// TODO - Figure out how to implement the map stuff for Android, ie query pins etc
		// TODO: Port to new conventions

		public MapGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.MapGalleryLegacy);
		}

		[Test]
		[Description ("Gallery element are present")]
		public void MapGalleryAllElementsPresent ()
		{
		//	CustomScrollDownToBottomForAllElements ();
		//	App.Screenshot ("All elements exist");
		}

		//[Test]
		//[Description ("Change MapMapType")]
		//public void MapGalleryMapType ()
		//{
		//	AppRect screenBounds = App.MainScreenBounds ();
		//	App.DragFromToForElement (5, q => q.Button ("Map Type"), screenBounds.Width - 15, screenBounds.Height - 100, screenBounds.Width - 15, 15);
		//	App.Tap (q => q.Button ("Map Type"));
		//	App.Screenshot ("Selected Map Type");
		//	App.Tap (q => q.Button ("Satellite"));
		//	App.Screenshot ("Satellite MapType selected");
		//	App.DragFromToForElement (5, PlatformQueries.SearchBars, screenBounds.Width - 15, 75, screenBounds.Width - 15, screenBounds.Height - 100);
		//}


//		[Test]
//		public void PinDetails ()
//		{
//			App.Tap (q => q.Raw(PlatformStrings.MapPin + " index:0"));
//			App.WaitForElement (q => q.Marked ("Sistine Chapel"));
//			App.WaitForElement (q => q.Marked ("Piazza della Rotunda, 00186 Rome, Province of Rome, Italy"));
//
//			App.Screenshot ("First pin");
//
//			App.Tap (q => q.Raw(PlatformStrings.MapPin + " index:1"));
//			App.WaitForElement (q => q.Marked ("Pantheon"));
//			App.WaitForElement (q => q.Marked ("Piazza del Colosseo, 00186 Rome, Province of Rome, Italy"));
//
//			App.Screenshot ("Second pin");
//
//			App.Tap (q => q.Raw(PlatformStrings.MapPin + " index:2"));
//			App.WaitForElement (q => q.Marked ("Colosseum"));
//			App.WaitForElement (q => q.Marked ("Piazza del Colosseo, 00184 Rome, Province of Rome, Italy"));
//
//			App.Screenshot ("Third pin");
//		}


		//void CustomScrollDownToBottomForAllElements ()
		//{
		//	AppRect screenBounds = App.MainScreenBounds ();

		//	App.DragFromToForElement (5, PlatformQueries.SearchBars, screenBounds.Width - 15, screenBounds.Height - 100, screenBounds.Width - 15, 15);
		//	App.DragFromToForElement (5, PlatformQueries.Map, screenBounds.Width - 15, screenBounds.Height - 100, screenBounds.Width - 15, 15);

		//	App.DragFromToForElement (5, q => q.Button ("Map Type"), screenBounds.Width - 15, screenBounds.Height - 100, screenBounds.Width - 15, 15);
		//	App.DragFromToForElement (5, q => q.Button ("Zoom In"), screenBounds.Width - 15, screenBounds.Height - 100, screenBounds.Width - 15, 15);
		//	App.DragFromToForElement (5, q => q.Button ("Zoom Out"), 15, screenBounds.Height - 100, 15, 15);
		//	App.DragFromToForElement (5, q => q.Button ("Address From Position"), screenBounds.Width - 15, screenBounds.Height - 100, screenBounds.Width - 15, 15);
		//}

/*******************************************************/
/**************** Landscape tests **********************/
/*******************************************************/

		//[Test]
		//[Description ("Gallery element are present - Landscape")]
		//public void MapGalleryAllElementsPresentLandscape ()
		//{
		//	App.SetOrientationLandscape ();
		//	App.Screenshot ("Rotated to Landscape");
		//	MapGalleryAllElementsPresent ();
		//	App.SetOrientationPortrait ();
		//	App.Screenshot ("Rotated to Portrait");
		//}

		//[Test]
		//[Description ("Change MapMapType - Landscape")]
		//public void MapGalleryMapTypeLandscape ()
		//{
		//	App.SetOrientationLandscape ();
		//	App.Screenshot ("Rotated to Landscape");
		//	MapGalleryMapType ();
		//	App.SetOrientationPortrait ();
		//	App.Screenshot ("Rotated to Portrait");
		//}
	}
}
