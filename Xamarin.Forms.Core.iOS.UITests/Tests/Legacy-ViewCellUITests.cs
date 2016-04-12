using NUnit.Framework;

using Xamarin.Forms.CustomAttributes;
using Xamarin.UITest;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("Cells")]
	internal class ViewCellGalleryTests : BaseTestFixture
	{
//		// TODO
		// TODO: Port to new conventions

		public ViewCellGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.ViewCellGalleryLegacy);
		}

//		public void AllElementsPresent ()
//		{
//			App.WaitForElement (q => q.Marked ("Testing"), "Timeout : Testing");
//			App.WaitForElement (q => q.Marked ("0"), "Timeout : 0");
//			App.WaitForElement (q => q.Marked ("BrandLabel"), "Timeout : BrandLabel");

//			App.Screenshot ("All elements exist");
//		}

		[Test]
		[UiTest (typeof(ViewCell))]
		[Description ("All elements exist")]
		public void ViewCellGalleryScrollDownForAllElements ()
		{
//			AllElementsPresent ();

//			App.ScrollForElement ("* marked:'0'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
//			App.ScrollForElement ("* marked:'1'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
//			App.ScrollForElement ("* marked:'2'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
//			App.ScrollForElement ("* marked:'3'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
//			App.ScrollForElement ("* marked:'4'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

//			App.Screenshot ("All ViewCells exist");
		}

//		[Test]
//		[Description ("All elements exist - Landscape")]
//		public void ViewCellGalleryScrollDownForAllElementsLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			ViewCellGalleryScrollDownForAllElements ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

	}
}
