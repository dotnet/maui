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

using System.Linq;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries
{
	internal static class IndexParser
	{
		static int ParseToken(string value)
		{
			if (!int.TryParse(value, out int index))
			{
				return -1;
			}

			return index;
		}

		public static bool ParseIndexes(string text, int count, out int[] indexes)
		{
			indexes = text.Split(',').Select(v => ParseToken(v.Trim())).ToArray();
			return indexes.Length == count;
		}
	}
}