using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries
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