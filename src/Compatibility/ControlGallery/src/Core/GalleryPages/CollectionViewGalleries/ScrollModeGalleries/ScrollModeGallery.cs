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

using System;
using System.Collections.Generic;
using System.Text;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.ScrollModeGalleries
{
	internal class ScrollModeGallery : ContentPage
	{
		public ScrollModeGallery()
		{
			var descriptionLabel =
					new Label { Text = "Scroll Mode Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Scroll Mode Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryBuilder.NavButton("Scroll Modes Testing", () =>
							new ScrollModeTestGallery(), Navigation)
					}
				}
			};
		}
	}
}
