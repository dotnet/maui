using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Xaml;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
{
	[XamlCompilation(XamlCompilationOptions.Skip)]
	public partial class SwipeViewVisualStatesCollectionGallery
	{
		public SwipeViewVisualStatesCollectionGallery()
		{
			InitializeComponent();
			BindingContext = new SwipeViewGalleryViewModel();

			WeakReferenceMessenger.Default.Register<SwipeViewGalleryViewModel, string>(this, "favourite", (_, sender) => { DisplayAlertAsync("SwipeView", "Favourite", "Ok"); });
			WeakReferenceMessenger.Default.Register<SwipeViewGalleryViewModel, string>(this, "delete", (_, sender) => { DisplayAlertAsync("SwipeView", "Delete", "Ok"); });
		}

		async void OnSwipeCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs args)
		{
			var currentSelection = args.CurrentSelection[0];
			SelectedLabel.Text = $"Current selection: {((Message)currentSelection).Title}";
			await DisplayAlertAsync("OnSwipeCollectionViewSelectionChanged", "CollectionView SelectionChanged", "Ok");
		}
	}
}