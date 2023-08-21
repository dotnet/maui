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

using Foundation;

namespace Microsoft.Maui.Controls.Compatibility.Platform.iOS
{
	internal static class IndexPathExtensions
	{
		public static bool IsLessThanOrEqualToPath(this NSIndexPath path, NSIndexPath otherPath)
		{
			if (path.Section < otherPath.Section)
			{
				return true;
			}

			if (path.Section == otherPath.Section)
			{
				return path.Item <= otherPath.Item;
			}

			return false;
		}

		public static NSIndexPath FindFirst(this NSIndexPath[] paths)
		{
			NSIndexPath firstPath = null;
			foreach (var path in paths)
			{
				if (firstPath == null)
				{
					firstPath = path;
					continue;
				}

				if (path.IsLessThanOrEqualToPath(firstPath))
				{
					firstPath = path;
				}
			}

			return firstPath;
		}
	}
}
