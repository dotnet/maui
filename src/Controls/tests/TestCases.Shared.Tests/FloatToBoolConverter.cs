namespace Microsoft.Maui.TestCases.Tests
{
	internal class FloatToBoolConverter : TypeConverter
	{
		public override bool CanConvertTo(object source, Type targetType)
		{
			if (targetType != typeof(bool) || !(source is float))
				return false;

			var flt = (float)source;
			var epsilon = 0.0001;
			if (Math.Abs(flt - 1.0f) < epsilon || Math.Abs(flt - 0.0f) < epsilon)
				return true;
			else
				return false;
		}

		public override object ConvertTo(object source, Type targetType)
		{
			var flt = (float)source;
			var epsilon = 0.0001;
			if (Math.Abs(flt - 1.0f) < epsilon)
				return true;
			else
				return false;
		}
	}
}