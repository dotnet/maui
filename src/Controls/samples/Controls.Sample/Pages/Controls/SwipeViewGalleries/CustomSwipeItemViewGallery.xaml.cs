using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	public partial class CustomSwipeItemViewGallery
	{
		public CustomSwipeItemViewGallery()
		{
			InitializeComponent();
			BindingContext = new SwipeViewGalleryViewModel();

			MessagingCenter.Subscribe<SwipeViewGalleryViewModel>(this, "favourite", sender => { DisplayAlert("SwipeView", "Favourite", "Ok"); });
			MessagingCenter.Subscribe<SwipeViewGalleryViewModel>(this, "delete", sender => { DisplayAlert("SwipeView", "Delete", "Ok"); });
		}
	}
}