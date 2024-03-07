using NUnit.Framework;

namespace UITests
{
	public class ScrollViewGalleryTests : ViewUITest
	{
		public ScrollViewGalleryTests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ScrollViewGallery);
		}
	}
}
