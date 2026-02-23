using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class SwipeHorizontalCollectionViewGallery : ContentPage
	{
		public SwipeHorizontalCollectionViewGallery()
		{
			InitializeComponent();
			BindingContext = new SwipeViewGalleryViewModel();

			WeakReferenceMessenger.Default.Register<SwipeViewGalleryViewModel, string>(this, "favourite", (_, sender) => { DisplayAlertAsync("SwipeView", "Favourite", "Ok"); });
			WeakReferenceMessenger.Default.Register<SwipeViewGalleryViewModel, string>(this, "delete", (_, sender) => { DisplayAlertAsync("SwipeView", "Delete", "Ok"); });
		}

		async void OnSwipeCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs args)
		{
			await DisplayAlertAsync("OnSwipeCollectionViewSelectionChanged", "CollectionView SelectionChanged", "Ok");
		}
	}
}