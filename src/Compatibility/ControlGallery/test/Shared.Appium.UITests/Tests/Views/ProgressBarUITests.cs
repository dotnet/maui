using NUnit.Framework;

namespace UITests
{
	public class ProgressBarUITests : ViewUITest
	{
		public ProgressBarUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ProgressBarGallery);
		}
	}
}
