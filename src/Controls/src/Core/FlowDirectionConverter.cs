using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
	/// <include file="../../docs/Microsoft.Maui.Controls/FlowDirectionConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.FlowDirectionConverter']/Docs" />
	public class FlowDirectionConverter : TypeConverter
	{
		/// <include file="../../docs/Microsoft.Maui.Controls/FlowDirectionConverter.xml" path="//Member[@MemberName='CanConvertFrom']/Docs" />
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		/// <include file="../../docs/Microsoft.Maui.Controls/FlowDirectionConverter.xml" path="//Member[@MemberName='CanConvertTo']/Docs" />
		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		/// <include file="../../docs/Microsoft.Maui.Controls/FlowDirectionConverter.xml" path="//Member[@MemberName='ConvertFrom']/Docs" />
		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (strValue != null)
			{
				if (Enum.TryParse(strValue, out FlowDirection direction))
					return direction;

				if (strValue.Equals("ltr", StringComparison.OrdinalIgnoreCase))
					return FlowDirection.LeftToRight;
				if (strValue.Equals("rtl", StringComparison.OrdinalIgnoreCase))
					return FlowDirection.RightToLeft;
				if (strValue.Equals("inherit", StringComparison.OrdinalIgnoreCase))
					return FlowDirection.MatchParent;
			}
			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", strValue, typeof(FlowDirection)));
		}

		/// <include file="../../docs/Microsoft.Maui.Controls/FlowDirectionConverter.xml" path="//Member[@MemberName='ConvertTo']/Docs" />
		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not FlowDirection direction)
				throw new NotSupportedException();
			return direction.ToString();
		}
	}
}