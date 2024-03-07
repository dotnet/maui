using NUnit.Framework;

namespace UITests
{
	public class SwitchUITests : ViewUITest
	{
		public SwitchUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.SwitchGallery);
		}
	}
}
