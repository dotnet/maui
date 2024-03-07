using UITest.Appium;

namespace UITests
{
	public class StepperUITests : ViewUITest
	{
		public StepperUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.StepperGallery);
		}

		protected override void FixtureTeardown()
		{
			App.Back();
			base.FixtureTeardown();
		}
	}
}
