using System;

namespace Xamarin.Forms
{
	[Xaml.TypeConversion(typeof(ImageSource))]
	public sealed class ImageSourceConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
				return Uri.TryCreate(value, UriKind.Absolute, out Uri uri) && uri.Scheme != "file" ? ImageSource.FromUri(uri) : ImageSource.FromFile(value);

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(ImageSource)));
		}

		public override string ConvertToInvariantString(object value)
		{
			if (value is FileImageSource fis)
				return fis.File;
			if (value is UriImageSource uis)
				return uis.Uri.ToString();
			throw new NotSupportedException();
		}
	}
}