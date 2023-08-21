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

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.SwipeViewGalleries
{
	public class SwipeThresholdGallery : ContentPage
	{
		public SwipeThresholdGallery()
		{
			Title = "SwipeThreshold Gallery";
			Content = new StackLayout
			{
				Children =
				{
					GalleryBuilder.NavButton("Horizontal SwipeThreshold Gallery", () => new HorizontalSwipeThresholdGallery(), Navigation),
					GalleryBuilder.NavButton("Vertical SwipeThreshold Gallery", () => new VerticalSwipeThresholdGallery(), Navigation),
					GalleryBuilder.NavButton("SwipeThreshold with Custom SwipeItem Gallery", () => new SwipeThresholdCustomSwipeItemGallery(), Navigation),
				}
			};
		}
	}
}