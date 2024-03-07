using NUnit.Framework;

namespace UITests
{
	public class CheckBoxUITests : ViewUITest
	{
		public CheckBoxUITests(TestDevice device) : base(device) { }

		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.CheckBoxGallery);
		}
	}
}
