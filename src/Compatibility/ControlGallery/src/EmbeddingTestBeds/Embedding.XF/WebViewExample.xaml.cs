using System;
using System.Collections.Generic;

using Xamarin.Forms;

namespace Embedding.XF
{
	public partial class WebViewExample : ContentPage
	{
		public WebViewExample()
		{
			InitializeComponent();

			var htmlSource = new HtmlWebViewSource();

			htmlSource.Html = @"<html><body>
<h1>Xamarin.Forms</h1>
<p>Welcome to WebView.</p>
</body>
</html>";
			wv.Source = htmlSource;
		}
	}
}

