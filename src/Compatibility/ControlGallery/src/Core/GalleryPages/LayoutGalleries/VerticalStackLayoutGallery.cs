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
	public class VerticalStackLayoutGallery : ContentPage
	{
		public VerticalStackLayoutGallery()
		{
			var layout = new VerticalStackLayout();

			for (int n = 0; n < 10; n++)
			{
				layout.Add(new Label { Text = $"Label {n} in a vertical stack" });
			}

			Content = layout;
		}
	}
}
