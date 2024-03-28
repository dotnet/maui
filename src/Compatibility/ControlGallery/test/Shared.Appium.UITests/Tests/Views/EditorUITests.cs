using UITest.Appium;

namespace UITests
{
	public class EditorUITests : ViewUITest
	{
		public EditorUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.EditorGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}
	}
}
