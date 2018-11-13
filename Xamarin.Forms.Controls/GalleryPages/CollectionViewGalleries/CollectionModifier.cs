using System;
using System.Collections.ObjectModel;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal abstract class CollectionModifier : ContentView
	{
		readonly CollectionView _cv;
		protected readonly Entry Entry;

		protected CollectionModifier (CollectionView cv, string buttonText)
		{
			_cv = cv;

			var layout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Fill
			};

			var button = new Button { Text = buttonText };
			var label = new Label { Text = "Index:", VerticalTextAlignment = TextAlignment.Center };

			Entry = new Entry { Keyboard = Keyboard.Numeric, Text = "0", WidthRequest = 200 };

			layout.Children.Add(label);
			layout.Children.Add(Entry);
			layout.Children.Add(button);

			button.Clicked += ButtonOnClicked;

			Content = layout;
		}

		void ButtonOnClicked(object sender, EventArgs e)
		{
			if (!ParseIndexes(out int[] indexes))
			{
				return;
			}

			if (!(_cv.ItemsSource is ObservableCollection<CollectionViewGalleryTestItem> observableCollection))
			{
				return;
			}

			ModifyCollection(observableCollection, indexes);
		}

		protected virtual bool ParseIndexes(out int[] indexes)
		{
			if (!int.TryParse(Entry.Text, out int index))
			{
				indexes = new int[0];
				return false;
			}

			indexes = new[] {index};
			return true;
		}

		protected abstract void ModifyCollection(ObservableCollection<CollectionViewGalleryTestItem> observableCollection, params int[] indexes);
	}
}