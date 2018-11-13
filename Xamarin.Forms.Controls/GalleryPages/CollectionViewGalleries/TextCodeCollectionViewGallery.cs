namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class TextCodeCollectionViewGallery : ContentPage
	{
		public TextCodeCollectionViewGallery(IItemsLayout itemsLayout)
		{
			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			var collectionView = new CollectionView
			{
				ItemsLayout = itemsLayout
			};

			var generator = new ItemsSourceGenerator(collectionView);

			layout.Children.Add(generator);
			layout.Children.Add(collectionView);
			Grid.SetRow(collectionView, 1);

			Content = layout;

			generator.GenerateItems();
		}
	}
}