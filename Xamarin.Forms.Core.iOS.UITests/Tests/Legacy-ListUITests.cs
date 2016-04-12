using NUnit.Framework;
using Xamarin.UITest;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("ListView")]
	internal class ListGalleryTests : BaseTestFixture
	{
		// TODO
		// TODO: Port to new conventions

		public ListGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.ListViewGalleryLegacy);
		}
//		void AllElementsExist ()
//		{
//			for (int i = 0; i < 5; i++)
//				App.WaitForElement (q => q.Marked (i.ToString ()));

//			App.Screenshot ("List elements exist");
//		}

		[Test]
		[Description ("Click 0 - 5 and reset")]
		public void ListGalleryClickElementsAndReset ()
		{
//			AllElementsExist ();

//			for (int i = 0; i < 5; i++) {
//				App.Tap (q => q.Raw (string.Format ("{0} index:{1}", PlatformStrings.Cell, i)));
//			}

//			App.WaitForNoElement (q => q.Marked ("0"), "Timeout : 0");
//			Assert.AreEqual (2, App.Query (q => q.Marked ("5")).Length);

//			App.Screenshot ("Clicked 0 - 5");

//			App.Tap (q => q.Raw (PlatformStrings.Cell + " index:5"));

//			App.WaitForElement (q => q.Marked ("0"), "Timeout : 0");
//			Assert.AreEqual (1, App.Query (q => q.Marked ("5")).Length);

//			App.Screenshot ("Reset elements");
		}
			
//		[Test]
//		[Description ("Scroll to the end of the list")]
//		public void ListGalleryScrollToEndOfList ()
//		{
//			AllElementsExist ();

//			for (int i = 0; i < 50; i++)
//				App.ScrollDownForElement (q => q.Marked (i.ToString ()), 2);

//			App.Screenshot ("At the bottom of the list");
//		}

//		[Test]
//		[Description ("Click 0 - 5 and reset - Landscape")]
//		public void ListGalleryClickElementsAndResetLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			ListGalleryClickElementsAndReset ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

//		[Test]
//		[Description ("Scroll to the end of the list - Landscape")]
//		public void ListGalleryScrollToEndOfListLandscape ()
//		{
//			App.SetOrientationLandscape ();
//			App.Screenshot ("Rotated to Landscape");
//			ListGalleryScrollToEndOfList ();
//			App.SetOrientationPortrait ();
//			App.Screenshot ("Rotated to Portrait");
//		}

	}
}
