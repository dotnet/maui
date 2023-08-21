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

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.LayoutGalleries
{
	public class HorizontalStackLayoutGallery : ContentPage
	{
		public HorizontalStackLayoutGallery()
		{
			var layout = new HorizontalStackLayout();

			for (int n = 0; n < 3; n++)
			{
				layout.Add(new Label { Text = $"Label {n} in a horizontal stack" });
			}

			Content = layout;
		}
	}
}
