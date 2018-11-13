using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Xamarin.Forms.Controls.GalleryPages.CollectionViewGalleries
{
	internal class DemoFilteredItemSource
	{
		readonly List<CollectionViewGalleryTestItem> _source;

		public ObservableCollection<CollectionViewGalleryTestItem> Items { get; }

		public DemoFilteredItemSource()
		{
			_source = new List<CollectionViewGalleryTestItem>();
			var count = 50;
			string[] images = 
			{
				"cover1.jpg", 
				"oasis.jpg",
				"photo.jpg",
				"Vegetables.jpg",
				"Fruits.jpg",
				"FlowerBuds.jpg",
				"Legumes.jpg"
			};

			for (int n = 0; n < count; n++)
			{
				_source.Add(new CollectionViewGalleryTestItem(DateTime.Now.AddDays(n),
					$"{images[n % images.Length]}, {n}", images[n % images.Length], n));
			}
			Items = new ObservableCollection<CollectionViewGalleryTestItem>(_source);
		}

		public void FilterItems(string filter)
		{
			var filteredItems = _source.Where(item => item.Caption.ToLower().Contains(filter.ToLower())).ToList();

			foreach (CollectionViewGalleryTestItem collectionViewGalleryTestItem in _source)
			{
				if (!filteredItems.Contains(collectionViewGalleryTestItem))
				{
					Items.Remove(collectionViewGalleryTestItem);
				}
				else
				{
					if (!Items.Contains(collectionViewGalleryTestItem))
					{
						Items.Add(collectionViewGalleryTestItem);
					}
				}
			}
		}
	}
}