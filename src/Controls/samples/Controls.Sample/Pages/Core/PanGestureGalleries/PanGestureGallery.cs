using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Pages
{
	[Preserve(AllMembers = true)]
	public class PanGestureGallery : ContentPage
	{
		public PanGestureGallery()
		{
			var descriptionLabel =
				new Label { Text = "PanGesture Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "PanGesture Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryBuilder.NavButton("PanGesture Playground", () =>
							new PanGesturePlaygroundGallery(), Navigation),
						GalleryBuilder.NavButton("PanGesture Events Gallery", () =>
							new PanGestureEventsGallery(), Navigation),
					}
				}
			};
		}
	}
}