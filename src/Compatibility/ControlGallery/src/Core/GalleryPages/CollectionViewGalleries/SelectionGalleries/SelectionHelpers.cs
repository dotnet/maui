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

using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.SelectionGalleries
{
	internal static class SelectionHelpers
	{
		public static string ToCommaSeparatedList(this IEnumerable<object> items)
		{
			if (items == null)
			{
				return string.Empty;
			}

			return string.Join(", ", items.Cast<CollectionViewGalleryTestItem>().Select(i => i.Caption));
		}
	}
}