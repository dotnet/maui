using NUnit.Framework;

namespace UITests
{
	public class CollectionViewUITests : ViewUITest
	{
		public CollectionViewUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.CollectionViewGallery);
		}
	}
}
