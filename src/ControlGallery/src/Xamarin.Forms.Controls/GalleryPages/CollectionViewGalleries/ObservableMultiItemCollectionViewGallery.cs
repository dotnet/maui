namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class ObservableMultiItemCollectionViewGallery : ContentPage
	{
		public ObservableMultiItemCollectionViewGallery(ItemsLayoutOrientation orientation = ItemsLayoutOrientation.Vertical,
			bool grid = true, int initialItems = 1000, bool withIndex = false)
		{
			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			var itemsLayout = grid
				? new GridItemsLayout(3, orientation)
				: new LinearItemsLayout(orientation) as IItemsLayout;

			var itemTemplate = ExampleTemplates.PhotoTemplate();

			var collectionView = new CollectionView { ItemsLayout = itemsLayout, ItemTemplate = itemTemplate, AutomationId = "collectionview" };

			var generator = new ItemsSourceGenerator(collectionView, initialItems, ItemsSourceType.MultiTestObservableCollection);

			var remover = new MultiItemRemover(collectionView, withIndex);

			var adder = new MultiItemAdder(collectionView, withIndex);
			var replacer = new MultiItemReplacer(collectionView);
			var mover = new MultiItemMover(collectionView);

			layout.Children.Add(generator);

			layout.Children.Add(remover);
			Grid.SetRow(remover, 1);

			layout.Children.Add(adder);
			Grid.SetRow(adder, 2);

			layout.Children.Add(replacer);
			Grid.SetRow(replacer, 3);

			layout.Children.Add(mover);
			Grid.SetRow(mover, 4);

			layout.Children.Add(collectionView);
			Grid.SetRow(collectionView, 5);

			Content = layout;

			generator.GenerateItems();
		}
	}
}