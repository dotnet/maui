using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls.Shapes
{
	/// <include file="../../../docs/Microsoft.Maui.Controls.Shapes/MatrixTypeConverter.xml" path="Type[@FullName='Microsoft.Maui.Controls.Shapes.MatrixTypeConverter']/Docs/*" />
	public class MatrixTypeConverter : TypeConverter
	{
		static readonly char[] _separator = [' ', ','];

		public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
			=> destinationType == typeof(string);

		public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
			=> CreateMatrix(value?.ToString());

		internal static Matrix CreateMatrix(string? value)
		{
			if (string.IsNullOrEmpty(value))
			{
				throw new ArgumentException("Argument is null or empty");
			}

			string[] strs = value!.Split(_separator, StringSplitOptions.RemoveEmptyEntries);

			if (strs.Length != 6)
			{
				throw new ArgumentException("Argument must have six numbers");
			}

			double[] values = new double[6];

			for (int i = 0; i < 6; i++)
			{
				if (!double.TryParse(strs[i], NumberStyles.Number, CultureInfo.InvariantCulture, out values[i]))
				{
					throw new ArgumentException("Argument must be numeric values");
				}
			}

			return new Matrix(values[0], values[1], values[2], values[3], values[4], values[5]);
		}

		public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
		{
			if (value is not Matrix matrix)
			{
				throw new NotSupportedException();
			}

			return $"{matrix.M11.ToString(CultureInfo.InvariantCulture)}, {matrix.M12.ToString(CultureInfo.InvariantCulture)}, {matrix.M21.ToString(CultureInfo.InvariantCulture)}, {matrix.M22.ToString(CultureInfo.InvariantCulture)}, {matrix.OffsetX.ToString(CultureInfo.InvariantCulture)}, {matrix.OffsetY.ToString(CultureInfo.InvariantCulture)}";
		}
	}
}
