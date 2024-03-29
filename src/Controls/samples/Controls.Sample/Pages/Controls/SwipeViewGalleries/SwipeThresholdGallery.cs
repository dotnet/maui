﻿using Maui.Controls.Sample.Pages;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
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