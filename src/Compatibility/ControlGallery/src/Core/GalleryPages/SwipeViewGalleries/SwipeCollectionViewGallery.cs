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
	public class SwipeCollectionViewGallery : ContentPage
	{
		public SwipeCollectionViewGallery()
		{
			Title = "CollectionView Galleries";
			Content = new StackLayout
			{
				Children =
				{
					GalleryBuilder.NavButton("Horizontal CollectionView Gallery", () => new SwipeHorizontalCollectionViewGallery(), Navigation),
					GalleryBuilder.NavButton("Vertical CollectionView Gallery", () => new SwipeVerticalCollectionViewGallery(), Navigation)
				}
			};
		}
	}
}