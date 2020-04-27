namespace Xamarin.Forms.Controls.GalleryPages.SwipeViewGalleries
{
	public partial class SwipeViewVisualStatesCollectionGallery : ContentPage
	{
		public SwipeViewVisualStatesCollectionGallery()
		{
			InitializeComponent();
			BindingContext = new SwipeViewGalleryViewModel();

			MessagingCenter.Subscribe<SwipeViewGalleryViewModel>(this, "favourite", sender => { DisplayAlert("SwipeView", "Favourite", "Ok"); });
			MessagingCenter.Subscribe<SwipeViewGalleryViewModel>(this, "delete", sender => { DisplayAlert("SwipeView", "Delete", "Ok"); });
		}

		async void OnSwipeCollectionViewSelectionChanged(object sender, SelectionChangedEventArgs args)
		{
			var currentSelection = args.CurrentSelection[0];
			SelectedLabel.Text = $"Current selection: {((Message)currentSelection).Title}";
			await DisplayAlert("OnSwipeCollectionViewSelectionChanged", "CollectionView SelectionChanged", "Ok");
		}
	}
}