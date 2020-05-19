using System;
using System.Collections.Generic;

using System.Maui;

namespace Embedding.XF
{
	public partial class WebViewExample : ContentPage
	{
		public WebViewExample()
		{
			InitializeComponent();

			var htmlSource = new HtmlWebViewSource();

			htmlSource.Html = @"<html><body>
<h1>System.Maui</h1>
<p>Welcome to WebView.</p>
</body>
</html>";
			wv.Source = htmlSource;
		}
	}
}

