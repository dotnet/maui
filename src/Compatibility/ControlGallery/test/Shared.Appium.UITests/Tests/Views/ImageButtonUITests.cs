using NUnit.Framework;

namespace UITests
{
	public class ImageButtonUITests : ViewUITest
	{
		public ImageButtonUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ImageButtonGallery);
		}
	}
}
