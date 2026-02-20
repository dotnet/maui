#nullable disable
using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <summary>
	/// Abstract class representing the source content for a <see cref="WebView"/>.
	/// </summary>
	[TypeConverter(typeof(WebViewSourceTypeConverter))]
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

		/// <summary>
		/// Loads the source content into the specified <paramref name="renderer"/>.
		/// </summary>
		/// <param name="renderer">The web view delegate that handles loading.</param>
		[EditorBrowsable(EditorBrowsableState.Never)]
		public abstract void Load(IWebViewDelegate renderer);

		internal event EventHandler SourceChanged;

		private sealed class WebViewSourceTypeConverter : TypeConverter
		{
			public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType) => false;
			public override object ConvertTo(ITypeDescriptorContext context, CultureInfo cultureInfo, object value, Type destinationType) => throw new NotSupportedException();

			public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
				=> sourceType == typeof(string) || sourceType == typeof(Uri);

			public override object ConvertFrom(ITypeDescriptorContext context, System.Globalization.CultureInfo culture, object value)
				=> value switch
				{
					string str => (UrlWebViewSource)str,
					Uri uri => (UrlWebViewSource)uri,
					_ => throw new NotSupportedException(),
				};
		}
	}
}