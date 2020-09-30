namespace Xamarin.Forms.Controls.GalleryPages.SwipeViewGalleries
{
	public partial class SwipeViewGestureRecognizerGallery : ContentPage
	{
		public SwipeViewGestureRecognizerGallery()
		{
			InitializeComponent();
			BindingContext = new SwipeViewGalleryViewModel();

			MessagingCenter.Subscribe<SwipeViewGalleryViewModel>(this, "favourite", sender => { DisplayAlert("SwipeView", "Favourite", "Ok"); });
			MessagingCenter.Subscribe<SwipeViewGalleryViewModel>(this, "delete", sender => { DisplayAlert("SwipeView", "Delete", "Ok"); });
			MessagingCenter.Subscribe<SwipeViewGalleryViewModel>(this, "tap", sender => { DisplayAlert("SwipeView", "TapGestureRecognizer", "Ok"); });
		}
	}
}