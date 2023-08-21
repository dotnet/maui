//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries
{
	internal class DemoFilteredItemSource
	{
		readonly List<CollectionViewGalleryTestItem> _source;
		private readonly Func<string, CollectionViewGalleryTestItem, bool> _filter;

		public ObservableCollection<CollectionViewGalleryTestItem> Items { get; }

		public DemoFilteredItemSource(int count = 50, Func<string, CollectionViewGalleryTestItem, bool> filter = null)
		{
			_source = new List<CollectionViewGalleryTestItem>();

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

			_filter = filter ?? ItemMatches;
		}

		private bool ItemMatches(string filter, CollectionViewGalleryTestItem item)
		{
			filter = filter ?? "";
			return item.Caption.IndexOf(filter, StringComparison.OrdinalIgnoreCase) != -1;
		}

		public void FilterItems(string filter)
		{
			var filteredItems = _source.Where(item => _filter(filter, item)).ToList();

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