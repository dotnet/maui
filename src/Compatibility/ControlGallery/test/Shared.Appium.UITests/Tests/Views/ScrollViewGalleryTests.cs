using UITest.Appium;

namespace UITests
{
	public class ScrollViewGalleryTests : ViewUITest
	{
		public ScrollViewGalleryTests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ScrollViewGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}
	}
}
