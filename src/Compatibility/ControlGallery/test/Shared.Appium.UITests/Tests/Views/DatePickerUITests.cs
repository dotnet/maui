using UITest.Appium;

namespace UITests
{
	public class DatePickerUITests : ViewUITest
	{
		public DatePickerUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.DatePickerGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}
	}
}
