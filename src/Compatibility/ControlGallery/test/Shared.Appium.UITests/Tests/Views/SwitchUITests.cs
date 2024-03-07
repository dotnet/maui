using UITest.Appium;

namespace UITests
{
	public class SwitchUITests : ViewUITest
	{
		public SwitchUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.SwitchGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}
	}
}
