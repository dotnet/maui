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

using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

#if WINDOWS
using WPoint = Windows.Foundation.Point;
using WPointCollection = Microsoft.UI.Xaml.Media.PointCollection;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
#else
using WPoint = System.Windows.Point;
using WPointCollection = System.Windows.Media.PointCollection;

namespace Microsoft.Maui.Controls.Compatibility.Platform.WPF
#endif
{
	public static class PointCollectionExtensions
	{
		public static WPointCollection ToWindows(this PointCollection pointCollection)
		{
			if (pointCollection == null || pointCollection.Count == 0)
			{
				return new WPointCollection();
			}

			WPointCollection points = new WPointCollection();
			Point[] array = new Point[pointCollection.Count];
			pointCollection.CopyTo(array, 0);

			for (int i = 0; i < array.Length; i++)
			{
				points.Add(new WPoint(array[i].X, array[i].Y));
			}

			return points;
		}
	}
}