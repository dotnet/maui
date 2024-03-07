using NUnit.Framework;

namespace UITests
{
	public class WebViewUITests : ViewUITest
	{
		public WebViewUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.WebViewGallery);
		}
	}
}