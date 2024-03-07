using NUnit.Framework;

namespace UITests
{
	public class LabelUITests : ViewUITest
	{
		public LabelUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.LabelGallery);
		}
	}
}
