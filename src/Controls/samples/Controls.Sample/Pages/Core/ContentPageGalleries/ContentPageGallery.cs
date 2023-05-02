using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Internals;

namespace Maui.Controls.Sample.Pages
{
	[Preserve(AllMembers = true)]
	public class ContentPageGallery : ContentPage
	{
		public ContentPageGallery()
		{
			var descriptionLabel =
				new Label { Text = "ContentPage Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "ContentPage Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
					{
						descriptionLabel,
						GalleryBuilder.NavButton("ContentPage BackgroundImage", () =>
							new ContentPageBackgroundImageGallery(), Navigation),
						GalleryBuilder.NavButton("ContentPage Background", () =>
							new ContentPageBackgroundGallery(), Navigation),
					}
				}
			};
		}
	}
}