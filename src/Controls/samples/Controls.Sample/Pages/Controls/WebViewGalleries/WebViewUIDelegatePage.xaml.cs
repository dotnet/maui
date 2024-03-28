using System;

namespace Maui.Controls.Sample.Pages.WebViewGalleries
{
	public partial class WebViewUIDelegatePage
	{
		public WebViewUIDelegatePage()
		{
			InitializeComponent();

			DelegateWebView.Source = new Uri("https://webrtc.github.io/samples/src/content/getusermedia/gum/");
		}
	}
}