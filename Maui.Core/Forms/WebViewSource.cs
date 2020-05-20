using System.ComponentModel;

namespace System.Maui
{
	[TypeConverter(typeof(WebViewSourceTypeConverter))]
	public abstract class WebViewSource 
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
			SourceChanged?.Invoke(this, EventArgs.Empty);
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		public abstract void Load(IWebViewDelegate renderer);

		internal event EventHandler SourceChanged;
	}
}