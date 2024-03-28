using UITest.Appium;

namespace UITests
{
	public class FrameUITests : ViewUITest
	{
		public FrameUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.FrameGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}
	}
}
