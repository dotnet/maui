using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public partial class ResourceSwipeItemsGallery : ContentPage
	{
		public ResourceSwipeItemsGallery()
		{
			InitializeComponent();
			BindingContext = new SwipeViewGalleryViewModel();

			MessagingCenter.Subscribe<SwipeViewGalleryViewModel>(this, "favourite", sender => { DisplayAlert("SwipeView", "Favourite", "Ok"); });
			MessagingCenter.Subscribe<SwipeViewGalleryViewModel>(this, "delete", sender => { DisplayAlert("SwipeView", "Delete", "Ok"); });
		}
	}
}