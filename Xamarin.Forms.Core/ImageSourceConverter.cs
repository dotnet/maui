using System;

namespace Xamarin.Forms
{
	[Xaml.TypeConversion(typeof(ImageSource))]
	public sealed class ImageSourceConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				Uri uri;
				return Uri.TryCreate(value, UriKind.Absolute, out uri) && uri.Scheme != "file" ? ImageSource.FromUri(uri) : ImageSource.FromFile(value);
			}

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(ImageSource)));
		}
	}
}