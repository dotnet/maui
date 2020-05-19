using System;
using System.Collections.Generic;
using System.Text;

namespace Xamarin.Forms
{
	[Xaml.TypeConversion(typeof(MediaSource))]
	public sealed class MediaSourceConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				Uri uri;
				return Uri.TryCreate(value, UriKind.Absolute, out uri) && uri.Scheme != "file" ? MediaSource.FromUri(uri) : MediaSource.FromFile(value);
			}

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(MediaSource)));
		}
	}
}
