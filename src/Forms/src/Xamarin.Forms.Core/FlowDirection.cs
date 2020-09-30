using System;

namespace Xamarin.Forms
{
	[TypeConverter(typeof(FlowDirectionConverter))]
	public enum FlowDirection
	{
		MatchParent = 0,
		LeftToRight = 1,
		RightToLeft = 2,
	}

	[Xaml.TypeConversion(typeof(FlowDirection))]
	public class FlowDirectionConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				if (Enum.TryParse(value, out FlowDirection direction))
					return direction;

				if (value.Equals("ltr", StringComparison.OrdinalIgnoreCase))
					return FlowDirection.LeftToRight;
				if (value.Equals("rtl", StringComparison.OrdinalIgnoreCase))
					return FlowDirection.RightToLeft;
				if (value.Equals("inherit", StringComparison.OrdinalIgnoreCase))
					return FlowDirection.MatchParent;
			}
			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(FlowDirection)));
		}
	}
}