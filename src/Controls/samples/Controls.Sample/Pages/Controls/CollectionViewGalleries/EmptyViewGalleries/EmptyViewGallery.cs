﻿using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.EmptyViewGalleries
{
	internal class EmptyViewGallery : ContentPage
	{
		public EmptyViewGallery()
		{
			var descriptionLabel =
				new Label { Text = "EmptyView Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "EmptyView Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryBuilder.NavButton("EmptyView (null ItemsSource)", () =>
							new EmptyViewNullGallery(), Navigation),
						GalleryBuilder.NavButton("EmptyView (null ItemsSource) View", () =>
							new EmptyViewNullGallery(false), Navigation),
						GalleryBuilder.NavButton("EmptyView (String)", () =>
							new EmptyViewStringGallery(), Navigation),
						GalleryBuilder.NavButton("EmptyView (View)", () =>
							new EmptyViewViewGallery(), Navigation),
						GalleryBuilder.NavButton("EmptyView (Template View)", () =>
							new EmptyViewTemplateGallery(), Navigation),
						GalleryBuilder.NavButton("EmptyView (Swap EmptyView)", () =>
							new EmptyViewSwapGallery(), Navigation),
						GalleryBuilder.NavButton("EmptyView (Data Template Selector)", () =>
						new EmptyViewWithDataTemplateSelector(), Navigation),
						GalleryBuilder.NavButton("EmptyView (load simulation)", () =>
							new EmptyViewLoadSimulateGallery(), Navigation),
						GalleryBuilder.NavButton("EmptyView RTL", () =>
							new EmptyViewRTLGallery(), Navigation)
					}
				}
			};
		}
	}
}