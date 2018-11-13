using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	[Preserve(AllMembers = true)]
	public class CollectionViewGalleryTestItem
	{
		public DateTime Date { get; set; }
		public string Caption { get; set; }
		public string Image { get; set; }
		public int Index { get; set; }

		public CollectionViewGalleryTestItem(DateTime date, string caption, string image, int index)
		{
			Date = date;
			Caption = caption;
			Image = image;
			Index = index;
		}

		public override string ToString()
		{
			return $"{Date:D}";
		}
	}

	internal class ItemsSourceGenerator : ContentView
	{
		readonly ItemsView _cv;
		readonly Entry _entry;

		public ItemsSourceGenerator(ItemsView cv, int initialItems = 1000)
		{
			_cv = cv;

			var layout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Fill
			};

			var button = new Button { Text = "Update" };
			var label = new Label { Text = "Item count:", VerticalTextAlignment = TextAlignment.Center };
			_entry = new Entry { Keyboard = Keyboard.Numeric, Text = initialItems.ToString(), WidthRequest = 200 };

			layout.Children.Add(label);
			layout.Children.Add(_entry);
			layout.Children.Add(button);

			button.Clicked += GenerateItems;

			Content = layout;
		}

		readonly string[] _images = 
		{
			"cover1.jpg", 
			"oasis.jpg",
			"photo.jpg",
			"Vegetables.jpg",
			"Fruits.jpg",
			"FlowerBuds.jpg",
			"Legumes.jpg"
		};

		public void GenerateItems()
		{
			if (int.TryParse(_entry.Text, out int count))
			{
				var items = new List<CollectionViewGalleryTestItem>();

				for (int n = 0; n < count; n++)
				{
					items.Add(new CollectionViewGalleryTestItem(DateTime.Now.AddDays(n),
						$"{_images[n % _images.Length]}, {n}", _images[n % _images.Length], n));
				}

				_cv.ItemsSource = items;
			}
		}

		public void GenerateObservableCollection()
		{
			if (int.TryParse(_entry.Text, out int count))
			{
				var items = new List<CollectionViewGalleryTestItem>();

				for (int n = 0; n < count; n++)
				{
					items.Add(new CollectionViewGalleryTestItem(DateTime.Now.AddDays(n),
						$"{_images[n % _images.Length]}, {n}", _images[n % _images.Length], n));
				}

				_cv.ItemsSource = new ObservableCollection<CollectionViewGalleryTestItem>(items);
			}
		}

		void GenerateItems(object sender, EventArgs e)
		{
			GenerateItems();
		}
	}
}