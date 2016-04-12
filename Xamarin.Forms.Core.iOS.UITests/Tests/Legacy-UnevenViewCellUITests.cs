using NUnit.Framework;
using Xamarin.UITest;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("Cells")]
	internal class UnevenViewCellGalleryTests : BaseTestFixture
	{
		// TODO
		// TODO: Port to new conventions

		public UnevenViewCellGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.UnevenViewCellGalleryLegacy);
		}
//		void AllElementsPresent ()
//		{
//			App.WaitForElement (q => q.Marked ("Testing"), "Timeout : Testing");
//			App.WaitForElement (PlatformQueries.Map, "Timeout : Map");
//			App.Screenshot ("All elements exist");
//		}

		[Test]
		[Description ("All views exist")]
		public void UnevenViewCellGalleryScrollDownForAllElements ()
		{
//			AllElementsPresent ();

//			var window = App.Query (q => q.Raw ("*")) [0];
//			var windowWidth = window.Rect.Width;
//			var windowHeight = window.Rect.Height;

//			App.DragFromToForElement (20, q => q.Marked ("1 day"), windowWidth - 100, windowHeight - 100, windowWidth - 100, windowHeight / 2);
//			App.DragFromToForElement (20, q => q.Marked ("2 days"), windowWidth - 100, windowHeight - 100, windowWidth - 100, windowHeight / 2);
//			App.DragFromToForElement (20, q => q.Marked ("3 days"), windowWidth - 100, windowHeight - 100, windowWidth - 100, windowHeight / 2);
//			App.DragFromToForElement (20, q => q.Marked ("4 days"), windowWidth - 100, windowHeight - 100, windowWidth - 100, windowHeight / 2);
//			App.DragFromToForElement (20, q => q.Marked ("5 days"), windowWidth - 100, windowHeight - 100, windowWidth - 100, windowHeight / 2);

//			App.Screenshot ("All views exist");
		}

//		[Test]
//		[Description ("Check uneven ViewCell sizes")]
//		public void UnevenViewCellGalleryCheckViewCellSizes ()
//		{
//			AllElementsPresent ();

//			var window = App.Query (q => q.Raw ("*")) [0];
//			var windowWidth = window.Rect.Width;
//			var windowHeight = window.Rect.Height;

//			var unevenCellHeight = App.Query (PlatformQueries.Map) [0].Rect.Height;

//			App.DragFromToForElement (20, q => q.Marked ("1 day"), windowWidth - 100, windowHeight - 100, windowWidth - 100, windowHeight / 2);

//			var evenCellHeight = App.Query (q => q.Marked ("1 day")) [0].Rect.Height;

//			Assert.Greater (unevenCellHeight, evenCellHeight);
//		}

//		[Test]
//		[Description ("All views exist - Landscape")]
//		public void UnevenViewCellGalleryScrollDownForAllElementsLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			UnevenViewCellGalleryScrollDownForAllElements ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Check uneven ViewCell sizes - Landscape")]
//		public void UnevenViewCellGalleryCheckViewCellSizesLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			UnevenViewCellGalleryCheckViewCellSizes ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

	}
}
