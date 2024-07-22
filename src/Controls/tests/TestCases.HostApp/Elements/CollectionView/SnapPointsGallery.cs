using System;
using Controls.Sample.UITests;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.CollectionViewGalleries
{
	internal class SnapPointsGallery : ContentPage
	{
		public SnapPointsGallery()
		{
			var descriptionLabel =
				new Label { Text = "Snap Points Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "Snap Points Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						TestBuilder.NavButton("Snap Points (Code, Horizontal List)", () =>
							new SnapPointsCodeGallery((LinearItemsLayout.Horizontal as ItemsLayout)!)!, Navigation),
						TestBuilder.NavButton("Snap Points (Code, Vertical List)", () =>
							new SnapPointsCodeGallery((LinearItemsLayout.Vertical as ItemsLayout)!)!, Navigation),
						TestBuilder.NavButton("Snap Points (Code, Horizontal Grid)", () =>
							new SnapPointsCodeGallery(new GridItemsLayout(2, ItemsLayoutOrientation.Horizontal)), Navigation),
						TestBuilder.NavButton("Snap Points (Code, Vertical Grid)", () =>
							new SnapPointsCodeGallery(new GridItemsLayout(2, ItemsLayoutOrientation.Vertical)), Navigation),
					}
				}
			};
		}
	}
}