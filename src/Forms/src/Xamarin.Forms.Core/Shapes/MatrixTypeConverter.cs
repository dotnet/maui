using System;
using System.Globalization;

namespace Xamarin.Forms.Shapes
{
	public class MatrixTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			return CreateMatrix(value);
		}

		internal static Matrix CreateMatrix(string value)
		{
			if (string.IsNullOrEmpty(value))
				throw new ArgumentException("Argument is null or empty");

			string[] strs = value.Split(new char[] { ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);

			if (strs.Length != 6)
				throw new ArgumentException("Argument must have six numbers");

			double[] values = new double[6];

			for (int i = 0; i < 6; i++)
				if (!double.TryParse(strs[i], NumberStyles.Number, CultureInfo.InvariantCulture, out values[i]))
					throw new ArgumentException("Argument must be numeric values");

			return new Matrix(values[0], values[1], values[2], values[3], values[4], values[5]);
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is Matrix matrix))
				throw new NotSupportedException();
			return $"{matrix.M11.ToString(CultureInfo.InvariantCulture)}, {matrix.M12.ToString(CultureInfo.InvariantCulture)}, {matrix.M21.ToString(CultureInfo.InvariantCulture)}, {matrix.M22.ToString(CultureInfo.InvariantCulture)}, {matrix.OffsetX.ToString(CultureInfo.InvariantCulture)}, {matrix.OffsetY.ToString(CultureInfo.InvariantCulture)}";
		}
	}
}