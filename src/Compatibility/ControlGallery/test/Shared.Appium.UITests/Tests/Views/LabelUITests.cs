using UITest.Appium;

namespace UITests
{
	public class LabelUITests : ViewUITest
	{
		public LabelUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.LabelGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}
	}
}
