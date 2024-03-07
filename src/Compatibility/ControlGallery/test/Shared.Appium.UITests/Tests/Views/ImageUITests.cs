using NUnit.Framework;

namespace UITests
{
	public class ImageUITests : ViewUITest
	{
		public ImageUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ImageGallery);
		}
	}
}
