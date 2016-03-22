using NUnit.Framework;
using Xamarin.UITest;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("ToolBar")]
	internal class ToolbarGalleryTests : BaseTestFixture
	{
		// TODO - Is there a ToolBar item limit, test image only toolbar item
		// TODO: Port to new conventions

		public ToolbarGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.ToolbarGalleryLegacy);
		}

		//void AllElementsPresent ()
		//{
		//	var elements = new [] { "One", "Two", "Three", "Four", "Click the toolbar" };
		//	foreach (var element in elements)
		//		App.WaitForElement (q => q.Marked (element));

		//	App.Screenshot ("All elements exist");
		//}

		[Test]
		public void ToolbarGalleryToolbarAction ()
		{
		//	AllElementsPresent ();

		//	var elements = new [] { "One", "Two", "Three", "Four" };
		//	foreach (var element in elements) {
		//		App.Tap (q => q.Marked (element));
		//		App.WaitForElement (q => q.Marked ("Activated: " + element));
		//	}

		//	App.Screenshot ("Toolbar commands fire");
		}

/*******************************************************/
/**************** Landscape tests **********************/
/*******************************************************/
	
	}
}
