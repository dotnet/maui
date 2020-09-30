using System;
using System.ComponentModel;

namespace Xamarin.Forms
{
	public abstract class WebViewSource : BindableObject
	{
		public static implicit operator WebViewSource(Uri url)
		{
			return new UrlWebViewSource { Url = url?.AbsoluteUri };
		}

		public static implicit operator WebViewSource(string url)
		{
			return new UrlWebViewSource { Url = url };
		}

		protected void OnSourceChanged()
		{
			EventHandler eh = SourceChanged;
			if (eh != null)
				eh(this, EventArgs.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public abstract void Load(IWebViewDelegate renderer);

		internal event EventHandler SourceChanged;
	}
}