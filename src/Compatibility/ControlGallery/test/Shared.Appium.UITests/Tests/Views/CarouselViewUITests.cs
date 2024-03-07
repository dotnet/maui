using NUnit.Framework;

namespace UITests
{
	public class CarouselViewUITests : ViewUITest
	{
		public CarouselViewUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.CarouselViewGallery);
		}
	}
}
