namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class ObservableCollectionGallery : ContentPage
	{
		public ObservableCollectionGallery()
		{
			var desc = "Observable Collection Galleries";

			var descriptionLabel = new Label { Text = desc, Margin = new Thickness(2, 2, 2, 2) };

			Title = "Simple DataTemplate Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,

						GalleryBuilder.NavButton("Filter Items", () => new FilterCollectionView(), Navigation),

						GalleryBuilder.NavButton("Add/Remove Items (List)", () =>
							new ObservableCodeCollectionViewGallery(grid: false), Navigation),

						GalleryBuilder.NavButton("Add/Remove Items (Grid)", () =>
							new ObservableCodeCollectionViewGallery(), Navigation),

						GalleryBuilder.NavButton("Add/Remove Items (Grid, initially empty)", () =>
							new ObservableCodeCollectionViewGallery(initialItems: 0), Navigation),

						GalleryBuilder.NavButton("Add/Remove Items (List, initially empty)", () =>
							new ObservableCodeCollectionViewGallery(grid: false, initialItems: 0), Navigation),

						GalleryBuilder.NavButton("Multi-item add/remove, no index",
							() => new ObservableMultiItemCollectionViewGallery(), Navigation),

						GalleryBuilder.NavButton("Multi-item add/remove, with index",
							() => new ObservableMultiItemCollectionViewGallery(withIndex: true), Navigation),

						GalleryBuilder.NavButton("Reset", () => new ObservableCollectionResetGallery(), Navigation),

						GalleryBuilder.NavButton("Add Items with timer to Empty Collection", () =>
							new ObservableCodeCollectionViewGallery(grid: false, initialItems: 0, addItemsWithTimer: true), Navigation)
					}
				}
			};
		}
	}
}