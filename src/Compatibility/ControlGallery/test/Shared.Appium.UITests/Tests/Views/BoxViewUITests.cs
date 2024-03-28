using UITest.Appium;

namespace UITests
{
	public class BoxViewUITests : ViewUITest
	{
		public BoxViewUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.BoxViewGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}
	}
}
