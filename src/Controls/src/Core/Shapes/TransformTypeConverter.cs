using System;

namespace Microsoft.Maui.Controls.Shapes
{
	public class TransformTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			return new MatrixTransform
			{
				Matrix = MatrixTypeConverter.CreateMatrix(value)
			};
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is MatrixTransform mt))
				throw new NotSupportedException();
			var converter = new MatrixTypeConverter();
			return converter.ConvertToInvariantString(mt.Matrix);
		}
	}
}