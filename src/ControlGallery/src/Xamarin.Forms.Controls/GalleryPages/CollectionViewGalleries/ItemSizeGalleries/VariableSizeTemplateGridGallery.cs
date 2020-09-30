namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.ItemSizeGalleries
{
	internal class VariableSizeTemplateGridGallery : ContentPage
	{
		public VariableSizeTemplateGridGallery(ItemsLayoutOrientation orientation = ItemsLayoutOrientation.Vertical)
		{
			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			var itemsLayout = new GridItemsLayout(2, orientation);

			var itemTemplate = ExampleTemplates.VariableSizeTemplate();

			var collectionView = new CollectionView
			{
				ItemsLayout = itemsLayout,
				ItemTemplate = itemTemplate,
				ItemSizingStrategy = ItemSizingStrategy.MeasureFirstItem
			};

			var generator = new ItemsSourceGenerator(collectionView, 100);

			var explanation = new Label();
			UpdateExplanation(explanation, collectionView.ItemSizingStrategy);

			var sizingStrategySelector = new EnumSelector<ItemSizingStrategy>(() => collectionView.ItemSizingStrategy,
				mode =>
				{
					collectionView.ItemSizingStrategy = mode;
					UpdateExplanation(explanation, collectionView.ItemSizingStrategy);
				});

			layout.Children.Add(generator);

			layout.Children.Add(sizingStrategySelector);
			Grid.SetRow(sizingStrategySelector, 1);

			layout.Children.Add(explanation);
			Grid.SetRow(explanation, 2);

			layout.Children.Add(collectionView);
			Grid.SetRow(collectionView, 3);

			Content = layout;

			generator.GenerateItems();
		}

		static void UpdateExplanation(Label explanation, ItemSizingStrategy strategy)
		{
			switch (strategy)
			{
				case ItemSizingStrategy.MeasureAllItems:
					explanation.Text = "Each item is individually measured.";
					break;
				case ItemSizingStrategy.MeasureFirstItem:
					explanation.Text = "The first item is measured, and that size is given to all subsequent cells.";
					break;
			}
		}
	}
}