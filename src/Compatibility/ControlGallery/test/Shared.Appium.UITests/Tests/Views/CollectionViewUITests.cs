using UITest.Appium;

namespace UITests
{
	public class CollectionViewUITests : ViewUITest
	{
		public CollectionViewUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.CollectionViewGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}
	}
}
