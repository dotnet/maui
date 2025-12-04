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
				Html = @"<!DOCTYPE html>
					<html>
					<head>
						<meta name='viewport' content='width=device-width, initial-scale=1.0'>
						<style>
							body { margin: 0; padding: 0; }
							.video-container { position: relative; padding-bottom: 56.25%; height: 0; overflow: hidden; }
							.video-container iframe { position: absolute; top: 0; left: 0; width: 100%; height: 100%; }
						</style>
					</head>
					<body>
						<div class='video-container'>
							<iframe src='https://www.youtube.com/embed/YE7VzlLtp-4' 
									frameborder='0' 
									allow='accelerometer; autoplay; clipboard-write; encrypted-media; gyroscope; picture-in-picture; web-share' 
									allowfullscreen>
							</iframe>
						</div>
					</body>
					</html>"
			};
		}
	}
}