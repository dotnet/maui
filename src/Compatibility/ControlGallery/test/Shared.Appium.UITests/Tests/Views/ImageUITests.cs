using UITest.Appium;

namespace UITests
{
	public class ImageUITests : ViewUITest
	{
		public ImageUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ImageGallery);

			// Let remote images load
			Thread.Sleep(2000);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}
	}
}
