using UITest.Appium;

namespace UITests
{
	public class SearchBarUITests : ViewUITest
	{
		public SearchBarUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.SearchBarGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}
	}
}
