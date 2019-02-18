namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class PropagateCodeGallery : ContentPage
	{
		public PropagateCodeGallery(IItemsLayout itemsLayout)
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

			var collectionView = new CollectionView
			{
				ItemsLayout = itemsLayout,
				ItemTemplate = itemTemplate,
			};

			var generator = new ItemsSourceGenerator(collectionView, initialItems: 2);
			layout.Children.Add(generator);
			var instructions = new Label();
			UpdateInstructions(layout, instructions);
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

				UpdateInstructions(layout, instructions);
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

		static void UpdateInstructions(Layout layout, Label instructions)
		{
			instructions.Text = $"The buttons in each item should be in order from {layout.FlowDirection}.";
		}
	}
}