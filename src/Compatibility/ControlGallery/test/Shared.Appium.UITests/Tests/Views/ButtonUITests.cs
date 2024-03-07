using NUnit.Framework;

namespace UITests
{
	public class ButtonUITests : ViewUITest
	{
		public ButtonUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ButtonGallery);
		}
	}
}
