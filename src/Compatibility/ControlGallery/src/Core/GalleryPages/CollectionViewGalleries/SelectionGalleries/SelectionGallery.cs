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

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.SelectionGalleries
{
	internal class SelectionGallery : ContentPage
	{
		public SelectionGallery()
		{
			var descriptionLabel =
				new Label { Text = "Selection Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Selection Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryBuilder.NavButton("Selection Modes", () =>
							new SelectionModeGallery(), Navigation),
						GalleryBuilder.NavButton("Preselected Item", () =>
							new PreselectedItemGallery(), Navigation),
						GalleryBuilder.NavButton("Preselected Items", () =>
							new PreselectedItemsGallery(), Navigation),
						GalleryBuilder.NavButton("Single Selection, Bound", () =>
							new SingleBoundSelection(), Navigation),
						GalleryBuilder.NavButton("Multiple Selection, Bound", () =>
							new MultipleBoundSelection(), Navigation),
						GalleryBuilder.NavButton("SelectionChangedCommandParameter", () =>
							new SelectionChangedCommandParameter(), Navigation),
						GalleryBuilder.NavButton("Filterable Single Selection", () =>
							new FilterSelection(), Navigation),
						GalleryBuilder.NavButton("Selection Synchronization", () =>
							new SelectionSynchronization(), Navigation),
					}
				}
			};
		}
	}
}