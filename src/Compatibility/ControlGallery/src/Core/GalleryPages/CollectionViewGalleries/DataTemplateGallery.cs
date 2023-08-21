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

using Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries.DataTemplateSelectorGalleries;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.CollectionViewGalleries
{
	internal class DataTemplateGallery : ContentPage
	{
		public DataTemplateGallery()
		{
			var descriptionLabel =
				new Label { Text = "Simple DataTemplate Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Simple DataTemplate Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryBuilder.NavButton("Vertical List (Code)", () =>
							new TemplateCodeCollectionViewGallery(LinearItemsLayout.Vertical), Navigation),
						GalleryBuilder.NavButton("Horizontal List (Code)", () =>
							new TemplateCodeCollectionViewGallery(LinearItemsLayout.Horizontal), Navigation),
						GalleryBuilder.NavButton("Vertical Grid (Code)", () =>
							new TemplateCodeCollectionViewGridGallery (), Navigation),
						GalleryBuilder.NavButton("Horizontal Grid (Code)", () =>
							new TemplateCodeCollectionViewGridGallery (ItemsLayoutOrientation.Horizontal), Navigation),
						GalleryBuilder.NavButton("DataTemplateSelector", () =>
							new DataTemplateSelectorGallery(), Navigation),
						GalleryBuilder.NavButton("Varied Size Data Templates", () =>
							new VariedSizeDataTemplateSelectorGallery(), Navigation),
					}
				}
			};
		}
	}
}