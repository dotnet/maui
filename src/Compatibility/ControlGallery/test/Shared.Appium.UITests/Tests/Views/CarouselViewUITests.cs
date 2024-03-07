using UITest.Appium;

namespace UITests
{
	public class CarouselViewUITests : ViewUITest
	{
		public CarouselViewUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.CarouselViewGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}
	}
}
