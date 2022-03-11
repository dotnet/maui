using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class SwipeViewGestureRecognizerGallery
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