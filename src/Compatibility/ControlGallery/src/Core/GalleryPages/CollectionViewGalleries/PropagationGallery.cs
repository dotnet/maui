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

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries
{
	internal class PropagationGallery : ContentPage
	{
		public PropagationGallery()
		{
			var descriptionLabel =
				new Label { Text = "Property Propagation Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Property Propagation Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryBuilder.NavButton("Propagate FlowDirection", () =>
							new PropagateCodeGallery(LinearItemsLayout.Vertical), Navigation),

						GalleryBuilder.NavButton("Propagate FlowDirection in EmptyView", () =>
							new PropagateCodeGallery(LinearItemsLayout.Vertical, 0), Navigation),
					}
				}
			};
		}
	}
}