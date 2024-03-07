namespace UITests
{
	public class ActivityIndicatorUITests : ViewUITest
	{
		public ActivityIndicatorUITests(TestDevice device) : base(device) { }
		
		protected override void NavigateToGallery()
		{
			App.NavigateToGallery(GalleryQueries.ActivityIndicatorGallery);
		}
	}
}