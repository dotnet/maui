using NUnit.Framework;

namespace UITests
{
	public class EntryUITests : ViewUITest
	{
		public EntryUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.EntryGallery);
		}
	}
}
