#nullable disable
using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls.Compatibility
{
	[Xaml.ProvideCompiled("Microsoft.Maui.Controls.XamlC.ConstraintTypeConverter")]
	public class ConstraintTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> false;

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			// IMPORTANT! Update ConstraintDesignTypeConverter.IsValid if making changes here
			var strValue = value?.ToString();

			if (strValue != null && double.TryParse(strValue, NumberStyles.Number, CultureInfo.InvariantCulture, out var size))
				return Constraint.Constant(size);

			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", strValue, typeof(Constraint)));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
			=> throw new NotSupportedException();
	}
}