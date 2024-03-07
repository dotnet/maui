using NUnit.Framework;

namespace UITests
{
	public class TimePickerUITests : ViewUITest
	{
		public TimePickerUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.TimePickerGallery);
		}
	}
}
