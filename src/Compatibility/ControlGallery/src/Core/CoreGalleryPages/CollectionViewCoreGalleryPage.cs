using System;
using System.Collections.Generic;

namespace Microsoft.Maui.Controls.ControlGallery
{
	internal class CollectionViewCoreGalleryPage : CoreGalleryPage<CollectionView>
	{
		protected override void InitializeElement(CollectionView element)
		{
			base.InitializeElement(element);

			var items = new List<string>();

			for (int n = 0; n < 1000; n++)
			{
				items.Add(DateTime.Now.AddDays(n).ToString("D"));
			}

			element.ItemsSource = items;

			element.HeightRequest = 250;

			element.ItemsLayout = LinearItemsLayout.Vertical;
		}
	}
}