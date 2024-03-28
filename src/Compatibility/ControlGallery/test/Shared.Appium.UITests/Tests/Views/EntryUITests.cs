using UITest.Appium;

namespace UITests
{
	public class EntryUITests : ViewUITest
	{
		public EntryUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.EntryGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}
	}
}
