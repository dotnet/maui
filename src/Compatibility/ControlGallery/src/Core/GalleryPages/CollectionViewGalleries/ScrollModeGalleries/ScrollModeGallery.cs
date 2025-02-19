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
