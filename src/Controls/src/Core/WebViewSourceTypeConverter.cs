using System;

namespace Microsoft.Maui.Controls
{
	[Xaml.TypeConversion(typeof(UrlWebViewSource))]
	public class WebViewSourceTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
				return new UrlWebViewSource { Url = value };

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(UrlWebViewSource)));
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is UrlWebViewSource uwvs))
				throw new NotSupportedException();
			return uwvs.Url;
		}
	}
}