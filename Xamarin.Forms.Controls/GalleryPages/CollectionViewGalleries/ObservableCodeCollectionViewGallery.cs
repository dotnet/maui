namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class ObservableCodeCollectionViewGallery : ContentPage
	{
		public ObservableCodeCollectionViewGallery(ItemsLayoutOrientation orientation = ItemsLayoutOrientation.Vertical,
			bool grid = true, int initialItems = 1000, bool addItemsWithTimer = false, ItemsUpdatingScrollMode scrollMode = ItemsUpdatingScrollMode.KeepItemsInView)
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
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			IItemsLayout itemsLayout = grid
				? new GridItemsLayout(3, orientation)
				: new LinearItemsLayout(orientation) as IItemsLayout;

			var itemTemplate = ExampleTemplates.PhotoTemplate();

			var collectionView = new CollectionView
			{
				ItemsLayout = itemsLayout,
				ItemTemplate = itemTemplate,
				AutomationId = "collectionview",
				Header = "This is the header",
				ItemsUpdatingScrollMode = scrollMode
			};

			var generator = new ItemsSourceGenerator(collectionView, initialItems, ItemsSourceType.ObservableCollection);

			var remover = new ItemRemover(collectionView);
			var adder = new ItemAdder(collectionView);
			var replacer = new ItemReplacer(collectionView);
			var mover = new ItemMover(collectionView);
			var inserter = new ItemInserter(collectionView);

			layout.Children.Add(generator);

			layout.Children.Add(remover);
			Grid.SetRow(remover, 1);

			layout.Children.Add(adder);
			Grid.SetRow(adder, 2);

			layout.Children.Add(replacer);
			Grid.SetRow(replacer, 3);

			layout.Children.Add(mover);
			Grid.SetRow(mover, 4);

			layout.Children.Add(inserter);
			Grid.SetRow(inserter, 5);

			layout.Children.Add(collectionView);
			Grid.SetRow(collectionView, 6);

			Content = layout;

			if (addItemsWithTimer)
				generator.GenerateEmptyObservableCollectionAndAddItemsEverySecond();
			else
				generator.GenerateItems();
		}
	}
}