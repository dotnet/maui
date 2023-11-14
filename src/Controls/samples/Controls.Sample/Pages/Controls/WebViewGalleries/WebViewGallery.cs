using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.WebViewGalleries
{
	public class WebViewGallery : ContentPage
	{
		public WebViewGallery()
		{
			Content = new ScrollView
			{
				Content = new StackLayout
				{
					Spacing = 5,
					Children =
					{
						GalleryBuilder.NavButton("WebView Playground", () => new WebViewPlayground(), Navigation),
#if MACCATALYST
						GalleryBuilder.NavButton("WebView using a custom UIDelegate", () => new WebViewUIDelegatePage(), Navigation),
#endif
					}
				}
			};
		}
	}
}