using System;
using System.Globalization;

namespace Xamarin.Forms
{
	[Xaml.TypeConversion(typeof(Size))]
	public class SizeTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				string[] wh = value.Split(',');
				if (wh.Length == 2
					&& double.TryParse(wh[0], NumberStyles.Number, CultureInfo.InvariantCulture, out double w)
					&& double.TryParse(wh[1], NumberStyles.Number, CultureInfo.InvariantCulture, out double h))
					return new Size(w, h);
			}

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(Size)));
		}
	}
}