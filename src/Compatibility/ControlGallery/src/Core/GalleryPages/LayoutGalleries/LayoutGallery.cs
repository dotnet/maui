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
	public class LayoutGallery : ContentPage
	{
		public LayoutGallery()
		{
			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						GalleryBuilder.NavButton("VerticalStackLayout Gallery", () => new VerticalStackLayoutGallery(), Navigation),
						GalleryBuilder.NavButton("HorizontalStackLayout Gallery", () => new HorizontalStackLayoutGallery(), Navigation),
					}
				}
			};
		}
	}
}
