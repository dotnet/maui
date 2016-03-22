using NUnit.Framework;

using Xamarin.Forms.CustomAttributes;
using Xamarin.UITest;

namespace Xamarin.Forms.Core.UITests
{
	[TestFixture]
	[Category ("CarouselPage")]
	internal class CarouselPageGalleryTests : BaseTestFixture
	{
		// TODO: Port to new conventions

		public CarouselPageGalleryTests ()
		{
			ShouldResetPerFixture = false;
		}

		protected override void NavigateToGallery ()
		{
			App.NavigateToGallery (GalleryQueries.CarouselPageGalleryLegacy);
		}

		[Test]
		[Description ("Swiping between screens")]
		[UiTest (typeof(CarouselPage))]
		public void CarouselPageGallerySwipeScreens ()
		{
			App.Screenshot ("At Gallery");

		//	var rootViewWidth = App.Query (q => q.Raw ("*"))[0].Rect.Width;
		//	var rootViewHeight = App.Query (q => q.Raw ("*"))[0].Rect.Height;

		//	// Verify the elements can be touched
		//	App.Tap (q => q.Button ("Click me one"));
		//	App.WaitForNoElement (q => q.Marked ("No click one"), "Timeout : No click one");
		//	App.WaitForElement (q => q.Marked ("Clicked one"), "Timeout : Clicked one");

		//	App.Pan (new Drag (ScreenBounds, rootViewWidth - 20, rootViewHeight / 2, 20, rootViewHeight / 2, Drag.Direction.RightToLeft));

		//	App.WaitForElement (q => q.Marked ("No click two"), "Timeout : No click two");
		//	App.WaitForElement (q => q.Button ("Click me two"), "Timeout : Click me two");
		//	App.Screenshot ("On page two");

		//	// Verify the elements can be touched
		//	App.Tap (q => q.Button ("Click me two"));
		//	App.WaitForNoElement (q => q.Marked ("No click two"), "Timeout : No click two");
		//	App.WaitForElement (q => q.Marked ("Clicked two"), "Timeout : Clicked two");

		//	App.Pan (new Drag (ScreenBounds, rootViewWidth - 20, rootViewHeight / 2, 20, rootViewHeight / 2, Drag.Direction.RightToLeft));

		//	App.WaitForElement (q => q.Marked ("No click three"), "Timeout : No click three");
		//	App.WaitForElement (q => q.Button ("Click me three"), "Timeout : Click me three");
		//	App.Screenshot ("On page three");

		//	// Verify the elements can be touched
		//	App.Tap (q => q.Button ("Click me three"));
		//	App.WaitForNoElement (q => q.Marked ("No click three"), "Timeout : No click three");
		//	App.WaitForElement (q => q.Marked ("Clicked three"), "Clicked three");
		//	App.Screenshot ("All screens interacted with");
		}
	}
}
