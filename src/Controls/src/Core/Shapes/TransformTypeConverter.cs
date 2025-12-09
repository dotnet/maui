using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/TransformTypeConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.TransformTypeConverter']/Docs/*" />
	public class TransformTypeConverter : TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
			=> new MatrixTransform
			{
				Matrix = MatrixTypeConverter.CreateMatrix(value?.ToString())
			};

		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is not MatrixTransform mt)
			{
				throw new NotSupportedException();
			}

			var converter = new MatrixTypeConverter();
			return converter.ConvertToInvariantString(mt.Matrix);
		}
	}
}
