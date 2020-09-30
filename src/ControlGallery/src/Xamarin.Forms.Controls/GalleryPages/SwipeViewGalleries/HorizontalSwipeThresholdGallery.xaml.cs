namespace Xamarin.Forms.Controls.GalleryPages.SwipeViewGalleries
{
	public partial class HorizontalSwipeThresholdGallery : ContentPage
	{
		public HorizontalSwipeThresholdGallery()
		{
			InitializeComponent();
		}

		void OnThresholdRevealSliderChanged(object sender, ValueChangedEventArgs args)
		{
			RevealThresholdSwipeView.Close();
		}

		void OnThresholdExecuteSliderChanged(object sender, ValueChangedEventArgs args)
		{
			ExecuteThresholdSwipeView.Close();
		}
	}
}