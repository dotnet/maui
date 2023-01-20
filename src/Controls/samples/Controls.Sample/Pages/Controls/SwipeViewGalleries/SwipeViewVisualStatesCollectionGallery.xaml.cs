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

#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Subscribe<SwipeViewGalleryViewModel>(this, "favourite", sender => { DisplayAlert("SwipeView", "Favourite", "Ok"); });
#pragma warning restore CS0618 // Type or member is obsolete
#pragma warning disable CS0618 // Type or member is obsolete
			MessagingCenter.Subscribe<SwipeViewGalleryViewModel>(this, "delete", sender => { DisplayAlert("SwipeView", "Delete", "Ok"); });
#pragma warning restore CS0618 // Type or member is obsolete
		}

		async void OnSwipeCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs args)
		{
			var currentSelection = args.CurrentSelection[0];
			SelectedLabel.Text = $"Current selection: {((Message)currentSelection).Title}";
			await DisplayAlert("OnSwipeCollectionViewSelectionChanged", "CollectionView SelectionChanged", "Ok");
		}
	}
}