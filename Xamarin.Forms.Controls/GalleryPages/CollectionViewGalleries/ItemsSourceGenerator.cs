using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries.CarouselViewGalleries;
using Xamarin.Forms.Internals;

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
		public event EventHandler<NotifyCollectionChangedEventArgs> CollectionChanged;
		readonly ItemsView _cv;
		private readonly ItemsSourceType _itemsSourceType;
		readonly Entry _entry;
		int _count = 0;

		CarouselView carousel => _cv as CarouselView;

		public int Count => _count;
		public ItemsSourceGenerator(ItemsView cv, int initialItems = 1000,
			ItemsSourceType itemsSourceType = ItemsSourceType.List)
		{
			_count = initialItems;
			_cv = cv;
			_itemsSourceType = itemsSourceType;
			var layout = new StackLayout
			{
				Orientation = StackOrientation.Horizontal,
				HorizontalOptions = LayoutOptions.Fill
			};

			var button = new Button { Text = "Update", AutomationId = "btnUpdate" };
			var label = new Label { Text = "Items:", VerticalTextAlignment = TextAlignment.Center };
			_entry = new Entry { Keyboard = Keyboard.Numeric, Text = initialItems.ToString(), WidthRequest = 100, AutomationId = "entryUpdate" };


			layout.Children.Add(label);
			layout.Children.Add(_entry);

			layout.Children.Add(button);

			button.Clicked += GenerateItems;
			MessagingCenter.Subscribe<ExampleTemplateCarousel>(this, "remove", (obj) =>
			{
				(cv.ItemsSource as ObservableCollection<CollectionViewGalleryTestItem>).Remove(obj.BindingContext as CollectionViewGalleryTestItem);
			});

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

		ObservableCollection<CollectionViewGalleryTestItem> _obsCollection;
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

				_obsCollection = new ObservableCollection<CollectionViewGalleryTestItem>(items);
				_count = _obsCollection.Count;
				CollectionChanged?.Invoke(this, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));

				_cv.ItemsSource = _obsCollection;
			}
		}

		void ObsItemsSource_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
		{
			CollectionChanged?.Invoke(sender, e);
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

			if (carousel == null)
				return;
		}
	}
}