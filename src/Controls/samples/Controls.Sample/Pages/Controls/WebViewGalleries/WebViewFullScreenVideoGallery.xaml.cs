using Microsoft.Maui.Controls;

namespace Maui.Controls.Sample.Pages.WebViewGalleries
{

	public partial class WebViewFullScreenVideoGallery : ContentPage
	{
		public WebViewFullScreenVideoGallery()
		{
			InitializeComponent();

			VideoWebView.Source = new HtmlWebViewSource
			{
				Html = "<html><iframe width=\"560\" height=\"315\" src=\"https://www.youtube.com/embed/YE7VzlLtp-4\" title=\"YouTube video player\" frameborder=\"0\" allow=\"accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share\" allowfullscreen></iframe></html>"
			};
		}
	}
}