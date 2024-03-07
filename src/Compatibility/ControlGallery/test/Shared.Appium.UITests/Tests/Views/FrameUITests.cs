using NUnit.Framework;

namespace UITests
{
	public class FrameUITests : ViewUITest
	{
		public FrameUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.FrameGallery);
		}
	}
}
