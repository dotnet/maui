using Microsoft.Maui;
using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.WebViewGalleries
{
	public class WebViewGalleries : ContentPage
	{
		public WebViewGalleries()
		{
			var descriptionLabel =
				   new Label { Text = "WebView Galleries", Margin = new Thickness(2, 2, 2, 2) };

			Title = "WebView Galleries";

			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Children =
				{
					descriptionLabel,
					GalleryBuilder.NavButton("WebView Gallery", () =>
						new WebViewGallery(), Navigation),
					GalleryBuilder.NavButton("WebView FullScreen Video", () =>
						new WebViewFullScreenVideoGallery(), Navigation),
				}
				}
			};
		}
	}
}