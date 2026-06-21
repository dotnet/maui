using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	public partial class CustomSwipeItemViewGallery
	{
		public CustomSwipeItemViewGallery()
		{
			InitializeComponent();
			BindingContext = new SwipeViewGalleryViewModel();

			WeakReferenceMessenger.Default.Register<SwipeViewGalleryViewModel, string>(this, "favourite", (_, sender) => { DisplayAlertAsync("SwipeView", "Favourite", "Ok"); });
			WeakReferenceMessenger.Default.Register<SwipeViewGalleryViewModel, string>(this, "delete", (_, sender) => { DisplayAlertAsync("SwipeView", "Delete", "Ok"); });
		}
	}
}