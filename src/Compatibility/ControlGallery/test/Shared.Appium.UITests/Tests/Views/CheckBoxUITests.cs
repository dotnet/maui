using UITest.Appium;

namespace UITests
{
	public class CheckBoxUITests : ViewUITest
	{
		public CheckBoxUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.CheckBoxGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}
	}
}
