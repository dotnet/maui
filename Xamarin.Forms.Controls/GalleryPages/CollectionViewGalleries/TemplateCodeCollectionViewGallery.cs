namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class TemplateCodeCollectionViewGallery : ContentPage
	{
		public TemplateCodeCollectionViewGallery(IItemsLayout itemsLayout)
		{
			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			var itemTemplate = ExampleTemplates.PhotoTemplate();

			var collectionView = new CollectionView
			{
				ItemsLayout = itemsLayout,
				ItemTemplate = itemTemplate
			};

			var generator = new ItemsSourceGenerator(collectionView, initialItems: 20);

			layout.Children.Add(generator);
			layout.Children.Add(collectionView);

			Grid.SetRow(collectionView, 1);

			Content = layout;

			generator.GenerateItems();
		}
	}
}