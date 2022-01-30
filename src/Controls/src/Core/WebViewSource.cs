using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/WebViewSource.xml" path="Type[@FullName='Microsoft.Maui.Controls.WebViewSource']/Docs" />
	public abstract class WebViewSource : BindableObject, IWebViewSource
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

		/// <include file="../../docs/Microsoft.Maui.Controls/WebViewSource.xml" path="//Member[@MemberName='Load']/Docs" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public abstract void Load(IWebViewDelegate renderer);

		internal event EventHandler SourceChanged;
	}
}