#nullable disable
using System;
using System.ComponentModel;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/WebViewSource.xml" path="Type[@FullName='Microsoft.Maui.Controls.WebViewSource']/Docs/*" />
	[ValueConverter(typeof(WebViewSourceValueConverter))]
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

		/// <include file="../../docs/Microsoft.Maui.Controls/WebViewSource.xml" path="//Member[@MemberName='Load']/Docs/*" />
		[EditorBrowsable(EditorBrowsableState.Never)]
		public abstract void Load(IWebViewDelegate renderer);

		internal event EventHandler SourceChanged;
	}

#nullable enable
	internal sealed class WebViewSourceValueConverter : IValueConverter
	{
		public object? Convert(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
			=> null;

		public object? ConvertBack(object? value, Type targetType, object? parameter, System.Globalization.CultureInfo culture)
			=> value switch
			{
				string url => (UrlWebViewSource)url,
				Uri url => (WebViewSource)url,
				_ => null,
			};
	}
}