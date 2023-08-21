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
	public class PositionGallery : ContentPage
	{
		public PositionGallery()
		{
			Title = "Position Galleries";
			Content = new StackLayout
			{
				Children =
				{
					GalleryBuilder.NavButton("SwipeItem Position Gallery", () => new SwipeItemPositionGallery(), Navigation),
					GalleryBuilder.NavButton("SwipeItemView Position Gallery", () => new SwipeItemViewPositionGallery(), Navigation)
				}
			};
		}
	}
}