using NUnit.Framework;

namespace UITests
{
	public class SearchBarUITests : ViewUITest
	{
		public SearchBarUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.SearchBarGallery);
		}
	}
}
