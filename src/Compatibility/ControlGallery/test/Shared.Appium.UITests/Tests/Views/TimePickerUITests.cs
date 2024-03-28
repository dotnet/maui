using UITest.Appium;

namespace UITests
{
	public class TimePickerUITests : ViewUITest
	{
		public TimePickerUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.TimePickerGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}
	}
}
