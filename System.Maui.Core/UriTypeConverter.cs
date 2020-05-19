using System;

namespace System.Maui
{
	[Xaml.ProvideCompiled("System.Maui.Core.XamlC.UriTypeConverter")]
	[Xaml.TypeConversion(typeof(Uri))]
	public class UriTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (string.IsNullOrWhiteSpace(value))
				return null;
			return new Uri(value, UriKind.RelativeOrAbsolute);
		}

		bool CanConvert(Type type)
		{
			if (type == typeof(string))
				return true;
			if (type == typeof(Uri))
				return true;

			return false;
		}
	}
}
