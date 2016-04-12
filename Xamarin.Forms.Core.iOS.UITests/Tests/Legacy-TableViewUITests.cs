using System;
using System.Runtime;
using NUnit.Framework;
using Xamarin.UITest;
using Xamarin.UITest.Android;
using Xamarin.UITest.iOS;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("TableView")]
	internal class TableViewGalleryTests : BaseTestFixture
	{

		// TODO: test sizes
		// TODO: Port to new conventions

		public TableViewGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.TableViewGalleryLegacy);
		}

		[Test]
		[Description ("Elements of section header are present")]
		public void TableViewGalleryHeader ()
		{
//			App.WaitForElement (q => q.Marked ("Section One"), "Timeout : Section One");
//			App.Screenshot ("Header is present");
		}

//		[Test]
//		[Description ("TableCells are present")]
//		public void TableViewGalleryTableCellsArePresent ()
//		{
//			var list = App.Query (PlatformQueries.Tables);
//			Assert.AreEqual (1, list.Length);
//			App.WaitForElement (q => q.Marked ("View Cell 1"), "Timeout : View Cell 1");
//			App.WaitForElement (q => q.Marked ("View Cell 2"), "Timeout : View Cell 2");

//			App.Screenshot ("TableCells are present");
//		}

//		[Test]
//		[Description ("Elements of CustomHeader are present - Landscape")]
//		public void TableViewGalleryCustomHeaderLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TableViewGalleryHeader ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("TableCells are present - Landscape")]
//		public void TableViewGalleryTableCellsArePresentLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			TableViewGalleryTableCellsArePresent ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

	}
}

