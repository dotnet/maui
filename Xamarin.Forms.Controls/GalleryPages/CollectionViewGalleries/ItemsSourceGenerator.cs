using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
    internal enum ItemsSourceType
	{
		List,
		ObservableCollection,
		MultiTestObservableCollection
	}

	internal class ItemsSourceGenerator : ContentView
	{
		readonly ItemsView _cv;
		private readonly ItemsSourceType _itemsSourceType;
		readonly Entry _entry;

		public ItemsSourceGenerator(ItemsView cv, int initialItems = 1000,
			ItemsSourceType itemsSourceType = ItemsSourceType.List)
		{
			_cv = cv;
			_itemsSourceType = itemsSourceType;
			var layout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Fill
			};

			var button = new Button { Text = "Update", AutomationId = "btnUpdate"  };
			var label = new Label { Text = "Item count:", VerticalTextAlignment = TextAlignment.Center };
			_entry = new Entry { Keyboard = Keyboard.Numeric, Text = initialItems.ToString(), WidthRequest = 200, AutomationId = "entryUpdate" };

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
			switch (_itemsSourceType)
			{
				case ItemsSourceType.List:
					GenerateList();
					break;
				case ItemsSourceType.ObservableCollection:
					GenerateObservableCollection();
					break;
				case ItemsSourceType.MultiTestObservableCollection:
					GenerateMultiTestObservableCollection();
					break;
			}
		}

		void GenerateList()
		{
			if (int.TryParse(_entry.Text, out int count))
			{
				var items = new List<CollectionViewGalleryTestItem>();

				for (int n = 0; n < count; n++)
				{
					items.Add(new CollectionViewGalleryTestItem(DateTime.Now.AddDays(n),
						$"Item: {n}", _images[n % _images.Length], n));
				}

				_cv.ItemsSource = items;
			}
		}

		void GenerateObservableCollection()
		{
			if (int.TryParse(_entry.Text, out int count))
			{
				var items = new List<CollectionViewGalleryTestItem>();

				for (int n = 0; n < count; n++)
				{
					items.Add(new CollectionViewGalleryTestItem(DateTime.Now.AddDays(n),
						$"Item: {n}", _images[n % _images.Length], n));
				}

				_cv.ItemsSource = new ObservableCollection<CollectionViewGalleryTestItem>(items);
			}
		}

		void GenerateMultiTestObservableCollection()
		{
			if (int.TryParse(_entry.Text, out int count))
			{
				var items = new MultiTestObservableCollection<CollectionViewGalleryTestItem>();

				for (int n = 0; n < count; n++)
				{
					items.Add(new CollectionViewGalleryTestItem(DateTime.Now.AddDays(n),
						$"{_images[n % _images.Length]}, {n}", _images[n % _images.Length], n));
				}

				_cv.ItemsSource = items;
			}
		}

		public void GenerateEmptyObservableCollectionAndAddItemsEverySecond()
		{
			if (int.TryParse(_entry.Text, out int count))
			{
				var items = new ObservableCollection<CollectionViewGalleryTestItem>();
				_cv.ItemsSource = items;
				Device.StartTimer(TimeSpan.FromSeconds(1), () =>
				{
					var n = items.Count + 1;
					items.Add(new CollectionViewGalleryTestItem(DateTime.Now.AddDays(n),
						$"{_images[n % _images.Length]}, {n}", _images[n % _images.Length], n));

					return !(count == items.Count);
				});
			}
		}


		void GenerateItems(object sender, EventArgs e)
		{
			GenerateItems();
		}
	}
}