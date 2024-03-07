using NUnit.Framework;

namespace UITests
{
	public class EditorUITests : ViewUITest
	{
		public EditorUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.EditorGallery);
		}
	}
}
