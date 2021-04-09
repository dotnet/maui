using System;
using System.ComponentModel;

namespace Microsoft.Maui
{
	public abstract class WebViewSource : IWebViewSource
	{
		public static implicit operator WebViewSource(Uri url)
		{
			return new UrlWebViewSource { Url = url?.AbsoluteUri };
		}

		public static implicit operator WebViewSource(string url)
		{
			return new UrlWebViewSource { Url = url };
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public abstract void Load(IWebViewDelegate webViewDelegate);
	}
}