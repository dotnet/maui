using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Pages.CollectionViewGalleries.CarouselViewGalleries
{
	[Preserve(AllMembers = true)]
	internal class CarouselViewGallery : ContentPage
	{
		public CarouselViewGallery()
		{
			var descriptionLabel =
				new Label { Text = "CarouselView Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "CarouselView Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel
					}
				}
			};
		}
	}
}