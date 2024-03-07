using NUnit.Framework;

namespace UITests
{
	internal class DatePickerUITests : ViewUITest
	{
		public DatePickerUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.DatePickerGallery);
		}
	}
}
