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

using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.SwipeViewGalleries
{
	[Preserve(AllMembers = true)]
	public class CustomSwipeItemGallery : ContentPage
	{
		public CustomSwipeItemGallery()
		{
			Title = "CustomSwipeItem Galleries";
			var layout = new StackLayout
			{
				Children =
				{
					GalleryBuilder.NavButton("Customize SwipeItem Gallery", () => new CustomizeSwipeItemGallery(), Navigation),
					GalleryBuilder.NavButton("No Icon or Text SwipeItem Gallery", () => new NoIconTextSwipeItemGallery(), Navigation)
				}
			};

			if (Device.RuntimePlatform != Device.WinUI)
			{
				layout.Children.Add(GalleryBuilder.NavButton("SwipeItemView Gallery", () => new CustomSwipeItemViewGallery(), Navigation));
				layout.Children.Add(GalleryBuilder.NavButton("CustomSwipeItem Size Gallery", () => new CustomSizeSwipeViewGallery(), Navigation));
			}

			Content = layout;
		}
	}
}