using UITest.Appium;

namespace UITests
{
	public class ProgressBarUITests : ViewUITest
	{
		public ProgressBarUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ProgressBarGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}
	}
}
