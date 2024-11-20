namespace Maui.Controls.Sample.CollectionViewGalleries
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
				ItemsLayout = itemsLayout,
				SelectionMode = SelectionMode.Single,
				AutomationId = "collectionview"
			};

			var generator = new ItemsSourceGenerator(collectionView);

			layout.Children.Add(generator);

			Grid.SetRow(collectionView, 1);
			layout.Children.Add(collectionView);

			Content = layout;

			generator.GenerateItems();
		}
	}
}