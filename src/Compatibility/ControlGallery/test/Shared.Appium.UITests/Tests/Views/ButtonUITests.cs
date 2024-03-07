using UITest.Appium;

namespace UITests
{
	public class ButtonUITests : ViewUITest
	{
		public ButtonUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ButtonGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}
	}
}
