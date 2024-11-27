namespace Microsoft.Maui.TestCases.Tests
{
	internal class StringToBoolConverter : TypeConverter
	{
		public override bool CanConvertTo(object source, Type targetType)
		{
			if (targetType != typeof(bool) || !(source is string))
				return false;

			var str = (string)source;
			str = str.ToLowerInvariant();

			switch (str)
			{
				case "0":
				case "1":
				case "false":
				case "true":
					return true;
				default:
					return false;
			}
		}

		public override object ConvertTo(object source, Type targetType)
		{
			var str = (string)source;
			str = str.ToLowerInvariant();

			return str switch
			{
				"1" or "true" => true,
				_ => (object)false,
			};
		}
	}
}