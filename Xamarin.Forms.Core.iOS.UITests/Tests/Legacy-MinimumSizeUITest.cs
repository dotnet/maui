using NUnit.Framework;
using Xamarin.UITest;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("StackLayout")]
	internal class MinimumSizeGalleryTests : BaseTestFixture
	{
		// TODO
		// TODO: Port to new conventions

		public MinimumSizeGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.MinimumSizeGalleryLegacy);
		}	
		

		[Test]
		[Description ("Scroll to the bottom of the TableView")]
		public void MinimumSizeGalleryTableViewElementsAreAccessible ()
		{
		//	AllElementsPresent ();

		//	var tableView = App.Query (PlatformQueries.Tables) [0];

		//	var tableX = tableView.Rect.X;
		//	var tableY = tableView.Rect.Y;
		//	var tableWidth = tableView.Rect.Width;
		//	var tableHeight = tableView.Rect.Height;

		//	var elementFound = App.DragFromToForElement (20, q => q.Marked ("Cell 4 Last"), tableWidth / 2, (tableY + tableHeight) - 70, tableWidth / 2, tableY + 30);

		//	Assert.IsTrue (elementFound);
		//	App.Screenshot ("All table elements exist");
		}

/*******************************************************/
/**************** Landscape tests **********************/
/*******************************************************/

	}
}
