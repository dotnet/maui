using System;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class SpanSetter : ContentView
	{
		readonly CollectionView _cv;
		readonly Entry _entry;

		public SpanSetter(CollectionView cv)
		{
			_cv = cv;

			var layout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Fill
			};

			var button = new Button { Text = "Update" };
			var label = new Label { Text = "Span:", VerticalTextAlignment = TextAlignment.Center };
			_entry = new Entry { Keyboard = Keyboard.Numeric, Text = "3", WidthRequest = 200 };

			layout.Children.Add(label);
			layout.Children.Add(_entry);
			layout.Children.Add(button);

			button.Clicked += UpdateSpan;

			Content = layout;
		}

		public void UpdateSpan()
		{
			if (int.TryParse(_entry.Text, out int span))
			{
				if (_cv.ItemsLayout is GridItemsLayout gridItemsLayout)
				{
					gridItemsLayout.Span = span;
				}
			}
		}

		public void UpdateSpan(object sender, EventArgs e)
		{
			UpdateSpan();
		}
	}
}