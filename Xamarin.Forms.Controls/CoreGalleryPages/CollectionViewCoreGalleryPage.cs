using System;
using System.Collections.Generic;

namespace Xamarin.Forms.Controls
{
	internal class CollectionViewCoreGalleryPage : CoreGalleryPage<CollectionView>
	{
		protected override void InitializeElement(CollectionView element)
		{
			base.InitializeElement(element);

			var items = new List<string>();

			for (int n = 0; n < 1000; n++)
			{
				items.Add(DateTime.Now.AddDays(n).ToLongDateString());
			}

			element.ItemsSource = items;

			element.HeightRequest = 250;

			element.ItemsLayout = ListItemsLayout.VerticalList;
		}
	}
}