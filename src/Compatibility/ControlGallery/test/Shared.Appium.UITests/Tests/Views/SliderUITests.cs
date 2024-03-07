using NUnit.Framework;

namespace UITests
{
	public class SliderUITests : ViewUITest
	{
		public SliderUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.SliderGallery);
		}
	}
}
