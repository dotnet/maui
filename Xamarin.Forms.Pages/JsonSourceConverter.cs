using System;

namespace Xamarin.Forms.Pages
{
	[Xaml.TypeConversion(typeof(JsonSource))]
	public class JsonSourceConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				value = value.Trim();
				Uri uri;
				if (Uri.TryCreate(value, UriKind.Absolute, out uri) && uri.Scheme != "file")
					return new UriJsonSource { Uri = uri };
				if (value.StartsWith("[", StringComparison.OrdinalIgnoreCase) || value.StartsWith("{", StringComparison.OrdinalIgnoreCase))
					return new StringJsonSource { Json = value };
			}

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(JsonSource)));
		}
	}
}