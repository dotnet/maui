using System;

namespace Microsoft.Maui.Controls
{
	public class AcceleratorTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value == null)
				return null;

			return Accelerator.FromString(value);
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is Accelerator acc))
				throw new NotSupportedException();
			return acc.ToString();
		}
	}
}
