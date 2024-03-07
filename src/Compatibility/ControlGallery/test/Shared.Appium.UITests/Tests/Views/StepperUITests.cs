using NUnit.Framework;

namespace UITests
{
	public class StepperUITests : ViewUITest
	{
		public StepperUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.StepperGallery);
		}
	}
}
