using System;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.SpacingGalleries
{
	internal class SpacingModifier : ContentView
	{
		protected readonly CollectionView _cv;
		protected readonly Entry Entry;

		public SpacingModifier(CollectionView cv, string buttonText)
		{
			_cv = cv;

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

		protected virtual string InitialEntryText => _cv.ItemsLayout is GridItemsLayout ? "0, 0" : "0";

		protected virtual void OnButtonClicked()
		{
			if (!ParseIndexes(out int[] indexes))
			{
				return;
			}

			if (_cv.ItemsLayout is ListItemsLayout listItemsLayout)
			{
				listItemsLayout.ItemSpacing = indexes[0];
			}
			else if (_cv.ItemsLayout is GridItemsLayout gridItemsLayout)
			{
				gridItemsLayout.VerticalItemSpacing = indexes[0];
				gridItemsLayout.HorizontalItemSpacing = indexes[1];
			}
		}

		protected virtual bool ParseIndexes(out int[] indexes)
		{
			if (_cv.ItemsLayout is ListItemsLayout listItemsLayout)
			{
				return IndexParser.ParseIndexes(Entry.Text, 1, out indexes);
			}

			return IndexParser.ParseIndexes(Entry.Text, 2, out indexes);
		}
	}
}
