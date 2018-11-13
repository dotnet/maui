namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class SnapPointsCodeGallery : ContentPage
	{
		public SnapPointsCodeGallery(ItemsLayout itemsLayout)
		{
			Title = $"Snap Points (Code, {itemsLayout})";

			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				}
			};

			itemsLayout.SnapPointsAlignment = SnapPointsAlignment.Start;
			itemsLayout.SnapPointsType = SnapPointsType.None;

			var itemTemplate = ExampleTemplates.SnapPointsTemplate();

			var collectionView = new CollectionView
			{
				ItemsLayout = itemsLayout,
				ItemTemplate = itemTemplate,
			};

			var generator = new ItemsSourceGenerator(collectionView, initialItems: 50);

			var snapPointsTypeSelector = new EnumSelector<SnapPointsType>(() => itemsLayout.SnapPointsType,
				type => itemsLayout.SnapPointsType = type);

			var snapPointsAlignmentSelector = new EnumSelector<SnapPointsAlignment>(() => itemsLayout.SnapPointsAlignment,
				type => itemsLayout.SnapPointsAlignment = type);

			var flowDirectionSelector = new EnumSelector<FlowDirection>(() => layout.FlowDirection,
				type => layout.FlowDirection = type);

			layout.Children.Add(generator);
			layout.Children.Add(snapPointsTypeSelector);
			layout.Children.Add(snapPointsAlignmentSelector);
			layout.Children.Add(flowDirectionSelector);
			layout.Children.Add(collectionView);

			Grid.SetRow(snapPointsTypeSelector, 1);
			Grid.SetRow(snapPointsAlignmentSelector, 2);
			Grid.SetRow(flowDirectionSelector, 3);
			Grid.SetRow(collectionView, 4);

			Content = layout;

			generator.GenerateItems();
		}
	}
}