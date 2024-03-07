using NUnit.Framework;

namespace UITests
{
	public class RadioButtonUITests : ViewUITest
	{
		public RadioButtonUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.RadioButtonGallery);
		}
	}
}
