using System;

namespace Xamarin.Forms
{
	[TypeConverter(typeof(TextAlignmentConverter))]
	public enum TextAlignment
	{
		Start,
		Center,
		End
	}

	[Xaml.TypeConversion(typeof(TextAlignment))]
	public class TextAlignmentConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				if (value.Equals("Start", StringComparison.OrdinalIgnoreCase) || value.Equals("left", StringComparison.OrdinalIgnoreCase))
					return TextAlignment.Start;
				if (value.Equals("top", StringComparison.OrdinalIgnoreCase))
					return TextAlignment.Start;
				if (value.Equals("right", StringComparison.OrdinalIgnoreCase))
					return TextAlignment.End;
				if (value.Equals("bottom", StringComparison.OrdinalIgnoreCase))
					return TextAlignment.End;
				if (value.Equals("center", StringComparison.OrdinalIgnoreCase))
					return TextAlignment.Center;
				if (value.Equals("middle", StringComparison.OrdinalIgnoreCase))
					return TextAlignment.Center;
				if (value.Equals("End", StringComparison.OrdinalIgnoreCase) || value.Equals("right", StringComparison.OrdinalIgnoreCase))
					return TextAlignment.End;
				if (value.Equals("Center", StringComparison.OrdinalIgnoreCase) || value.Equals("center", StringComparison.OrdinalIgnoreCase))
					return TextAlignment.Center;

				if (Enum.TryParse(value, out TextAlignment direction))
					return direction;
			}
			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(TextAlignment)));
		}
	}
}