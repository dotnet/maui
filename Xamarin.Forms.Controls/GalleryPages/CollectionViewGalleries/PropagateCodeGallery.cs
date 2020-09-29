namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class PropagateCodeGallery : ContentPage
	{
		public PropagateCodeGallery(IItemsLayout itemsLayout, int itemsCount = 2)
		{
			Title = $"Propagate FlowDirection=RTL";

			var layout = new Grid
			{
				RowDefinitions = new RowDefinitionCollection
				{
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Auto },
					new RowDefinition { Height = GridLength.Star }
				},
				FlowDirection = FlowDirection.RightToLeft,
				Visual = VisualMarker.Material
			};

			var itemTemplate = ExampleTemplates.PropagationTemplate();

			var emptyView = ExampleTemplates.PropagationTemplate().CreateContent() as View;


			var collectionView = new CollectionView
			{
				ItemsLayout = itemsLayout,
				ItemTemplate = itemTemplate,
				EmptyView = emptyView
			};

			var generator = new ItemsSourceGenerator(collectionView, initialItems: itemsCount);
			layout.Children.Add(generator);
			var instructions = new Label();
			UpdateInstructions(layout, instructions, itemsCount == 0);
			Grid.SetRow(instructions, 2);
			layout.Children.Add(instructions);

			var switchLabel = new Label { Text = "Toggle FlowDirection" };
			var switchLayout = new StackLayout { Orientation = StackOrientation.Horizontal };
			var updateSwitch = new Switch { };

			updateSwitch.Toggled += (sender, args) =>
			{
				layout.FlowDirection = layout.FlowDirection == FlowDirection.RightToLeft
					? FlowDirection.LeftToRight
					: FlowDirection.RightToLeft;

				UpdateInstructions(layout, instructions, itemsCount == 0);
			};

			switchLayout.Children.Add(switchLabel);
			switchLayout.Children.Add(updateSwitch);

			Grid.SetRow(switchLayout, 1);
			layout.Children.Add(switchLayout);

			layout.Children.Add(collectionView);

			Grid.SetRow(collectionView, 3);

			Content = layout;

			generator.GenerateItems();
		}

		static void UpdateInstructions(Layout layout, Label instructions, bool isEmpty)
		{
			if (isEmpty)
			{
				instructions.Text = $"The buttons in the empty view should be in order from {layout.FlowDirection}.";
			}
			else
			{
				instructions.Text = $"The buttons in each item should be in order from {layout.FlowDirection}.";
			}
		}
	}
}