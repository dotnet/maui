using Controls.Sample.UITests;
using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.CollectionViewGalleries.SelectionGalleries
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
						TestBuilder.NavButton("Selection Modes", () =>
							new SelectionModeGallery(), Navigation),
						TestBuilder.NavButton("Preselected Item", () =>
							new PreselectedItemGallery(), Navigation),
						TestBuilder.NavButton("Preselected Items", () =>
							new PreselectedItemsGallery(), Navigation),
						TestBuilder.NavButton("Single Selection, Bound", () =>
							new SingleBoundSelection(), Navigation),
						TestBuilder.NavButton("Multiple Selection, Bound", () =>
							new MultipleBoundSelection(), Navigation),
						TestBuilder.NavButton("SelectionChangedCommandParameter", () =>
							new SelectionChangedCommandParameter(), Navigation),
						TestBuilder.NavButton("Filterable Single Selection", () =>
							new FilterSelection(), Navigation),
						TestBuilder.NavButton("Selection Synchronization", () =>
							new SelectionSynchronization(), Navigation),
						TestBuilder.NavButton("Visual states", () =>
							new VisualStatesGallery(), Navigation),
					}
				}
			};
		}
	}
}