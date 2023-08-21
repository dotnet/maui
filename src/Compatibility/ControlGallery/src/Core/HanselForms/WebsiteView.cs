//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;

using Microsoft.Maui.Controls;

namespace Microsoft.Maui.Controls.ControlGallery
{
	public class WebsiteView : BaseView
	{
		public WebsiteView(string site, string title)
		{
			this.Title = title;
			var webView = new WebView();
			webView.Source = new UrlWebViewSource
			{
				Url = site
			};
			Content = webView;
		}
	}
}

