using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class ResourceSwipeItemsGallery : ContentPage
	{
		public ResourceSwipeItemsGallery()
		{
			InitializeComponent();
			BindingContext = new SwipeViewGalleryViewModel();

			WeakReferenceMessenger.Default.Register<SwipeViewGalleryViewModel, string>(this, "favourite", (_, sender) => { DisplayAlertAsync("SwipeView", "Favourite", "Ok"); });
			WeakReferenceMessenger.Default.Register<SwipeViewGalleryViewModel, string>(this, "delete", (_, sender) => { DisplayAlertAsync("SwipeView", "Delete", "Ok"); });
		}
	}
}