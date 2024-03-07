using UITest.Appium;

namespace UITests
{
	public class ImageButtonUITests : ViewUITest
	{
		public ImageButtonUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ImageButtonGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}
	}
}
