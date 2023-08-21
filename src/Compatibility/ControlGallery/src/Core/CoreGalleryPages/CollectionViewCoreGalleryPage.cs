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