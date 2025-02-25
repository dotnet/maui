namespace Maui.Controls.Sample.CollectionViewGalleries.SpacingGalleries
{
	internal class SpacingModifier : ContentView
	{
		protected readonly Entry Entry;
		protected readonly IItemsLayout ItemsLayout;

		public SpacingModifier(IItemsLayout itemsLayout, string buttonText)
		{
			ItemsLayout = itemsLayout;

			var layout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Fill
			};

			var button = new Button { Text = buttonText, AutomationId = $"btn{buttonText}" };
			var label = new Label { Text = LabelText, VerticalTextAlignment = TextAlignment.Center };

			Entry = new Entry { Text = InitialEntryText, WidthRequest = 100, AutomationId = $"entry{buttonText}" };

			layout.Children.Add(label);
			layout.Children.Add(Entry);
			layout.Children.Add(button);

			button.Clicked += ButtonOnClicked;

			Content = layout;
		}

		void ButtonOnClicked(object sender, EventArgs e)
		{
			OnButtonClicked();
		}

		protected virtual string LabelText => "Spacing:";

		protected virtual string InitialEntryText => ItemsLayout is GridItemsLayout ? "0, 0" : "0";

		protected virtual void OnButtonClicked()
		{
			if (!ParseIndexes(out int[] indexes))
			{
				return;
			}

			if (ItemsLayout is LinearItemsLayout listItemsLayout)
			{
				listItemsLayout.ItemSpacing = indexes[0];
			}
			else if (ItemsLayout is GridItemsLayout gridItemsLayout)
			{
				gridItemsLayout.VerticalItemSpacing = indexes[0];
				gridItemsLayout.HorizontalItemSpacing = indexes[1];
			}
		}

		protected virtual bool ParseIndexes(out int[] indexes)
		{
			if (ItemsLayout is LinearItemsLayout listItemsLayout)
			{
				return IndexParser.ParseIndexes(Entry.Text, 1, out indexes);
			}

			return IndexParser.ParseIndexes(Entry.Text, 2, out indexes);
		}
	}
}
