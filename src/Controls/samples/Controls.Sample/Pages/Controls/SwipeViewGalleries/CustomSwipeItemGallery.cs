// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Devices;

namespace Maui.Controls.Sample.Pages.SwipeViewGalleries
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

			if (DeviceInfo.Platform != DevicePlatform.WinUI)
			{
				layout.Children.Add(GalleryBuilder.NavButton("SwipeItemView Gallery", () => new CustomSwipeItemViewGallery(), Navigation));
				layout.Children.Add(GalleryBuilder.NavButton("CustomSwipeItem Size Gallery", () => new CustomSizeSwipeViewGallery(), Navigation));
			}

			Content = layout;
		}
	}
}