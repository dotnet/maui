using NUnit.Framework;

namespace UITests
{
	public class BoxViewUITests : ViewUITest
	{
		public BoxViewUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.BoxViewGallery);
		}
	}
}
