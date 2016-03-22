using NUnit.Framework;

using Xamarin.Forms.CustomAttributes;
using Xamarin.UITest;
using Xamarin.UITest.Queries;
using System.Threading;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("GridLayout")]
	internal class GridLayoutGalleryTests : BaseTestFixture
	{
		// TODO - test Absolutes
		// TODO: Port to new conventions

		public GridLayoutGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.GridGalleryLegacy);
		}

		//void AllElementsPresent ()
		//{
		//	App.ScrollForElement ("* marked:'Column Types:'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
		//	App.ScrollForElement ("* marked:'Absolute Width'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
		//	App.ScrollForElement ("* marked:'Auto Width'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
		//	App.ScrollForElement ("* marked:'Star'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
		//	App.ScrollForElement ("* marked:'*'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
		//	App.ScrollForElement ("* marked:'**'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
		//	App.ScrollForElement ("* marked:'***'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
		//	App.ScrollForElement ("* marked:'Right'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
		//	App.ScrollForElement ("* marked:'Center'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
		//	App.ScrollForElement ("* marked:'Left'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
		//	App.ScrollForElement ("* marked:'Fill'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
		//	App.ScrollForElement ("* marked:'Spans:'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
		//	App.ScrollForElement ("* marked:'Spanning 4 columns'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
		//	App.ScrollForElement ("* marked:'Spanning 3 rows'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
		//	App.ScrollForElement ("* marked:'Spanning 4 columns'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));
		//	App.ScrollForElement ("* marked:'a block 3x3'", new Drag (ScreenBounds, Drag.Direction.BottomToTop, Drag.DragLength.Medium));

		//	App.Screenshot ("All elements present");
		//}


		[Test]
		[Category("ManualReview")]
		[Description ("Check Grid Star sizing")]
		[UiTest (typeof(Grid))]
		public void GridLayoutGalleryStarLayout ()
		{
		//	AllElementsPresent ();

		//	App.ScrollForElement ("* marked:'*'", new Drag (ScreenBounds, Drag.Direction.TopToBottom, Drag.DragLength.Medium));

		//	var oneStarWidth = App.Query (q => q.Marked ("*"))[0].Rect.Width;
		//	var twoStarWidth = App.Query (q => q.Marked ("**"))[0].Rect.Width;
		//	var threeStarWidth = App.Query (q => q.Marked ("***"))[0].Rect.Width;

		//	Assert.AreEqual (threeStarWidth, oneStarWidth * 3, 2.0);
		//	Assert.AreEqual (twoStarWidth, oneStarWidth * 2, 2.0);

		//	App.Screenshot ("Star layout correct");
		}

		// TODO port to new framework
		//[Test]
		//[UiTest (Test.Layouts.Grid)]
		//public void GridLayoutGallerySpanSizes ()
		//{
		//	AllElementsPresent ();

		//	var unitWidth = App.Query (q => q.Marked ("Unit"))[0].Rect.Width;
		//	var unitHeight = App.Query (q => q.Marked ("Unit"))[0].Rect.Height;

		//	var spanningFourColumnsWidth = App.Query (q => q.Marked ("Spanning 4 columns"))[0].Rect.Width; 
		//	var spanningFourColumnsHeight = App.Query (q => q.Marked ("Spanning 4 columns"))[0].Rect.Height; 

		//	// platform queries deal with label rendering differences
		//	var threeXThreeWidth = App.Query (PlatformQueries.ThreeXThreeGridCell) [0].Rect.Width;
		//	var threeXThreeHeight = App.Query (PlatformQueries.ThreeXThreeGridCell) [0].Rect.Height;

		//	var spanningThreeRowsWidth = App.Query (PlatformQueries.SpanningThreeRows)[0].Rect.Width; 
		//	var spanningThreeRowsHeight = App.Query (PlatformQueries.SpanningThreeRows)[0].Rect.Height;

		//	Assert.AreEqual (spanningFourColumnsWidth, unitWidth * 4, 2.0);
		//	Assert.AreEqual (spanningFourColumnsHeight, unitHeight, 2.0);

		//	Assert.AreEqual (threeXThreeWidth, unitWidth * 3, 2.0);
		//	Assert.AreEqual (threeXThreeHeight, unitHeight * 3, 2.0);

		//	Assert.AreEqual (spanningThreeRowsWidth, unitWidth, 2.0);
		//	Assert.AreEqual (spanningThreeRowsHeight, unitHeight * 3, 2.0);

		//	App.Screenshot ("Span sizes correct");
		//}

		// TODO port to new framework
		//[Test]
		//[UiTest (Test.Layouts.Grid)]
		//public void GridLayoutGalleryResizesProperlyAfterRotation ()
		//{
		//	// Displays GridLayout bug on rotation (Issue #854)
		//	AllElementsPresent ();

		//	AppRect detailBounds = App.DetailPage ().Rect;

		//	var oneStarWidth = App.Query (q => q.Marked ("*"))[0].Rect.Width;
		//	var twoStarWidth = App.Query (q => q.Marked ("**"))[0].Rect.Width;
		//	var threeStarWidth = App.Query (q => q.Marked ("***"))[0].Rect.Width;

		//	Assert.AreEqual (detailBounds.Width, oneStarWidth + twoStarWidth + threeStarWidth, 1.0);
		//	App.Screenshot ("All stars fill portrait screen width");

		//	App.SetOrientationLandscape ();

		//	AppRect detailBoundsAfterRotation = App.DetailPage ().Rect;
		//	var oneStarWidthAfterRotation = App.Query (q => q.Marked ("*"))[0].Rect.Width;
		//	var twoStarWidthAfterRotation = App.Query (q => q.Marked ("**"))[0].Rect.Width;
		//	var threeStarWidthAfterRotation = App.Query (q => q.Marked ("***"))[0].Rect.Width;

		//	Assert.AreEqual (detailBoundsAfterRotation.Width, oneStarWidthAfterRotation + twoStarWidthAfterRotation + threeStarWidthAfterRotation, 1.0);
		//	App.Screenshot ("Grid stars resized");

		//	App.SetOrientationPortrait ();
		//}
	}
}
