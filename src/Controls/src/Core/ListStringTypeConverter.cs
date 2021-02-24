using System;
using System.Collections.Generic;
using System.Linq;

namespace Microsoft.Maui.Controls
{
	[Xaml.ProvideCompiled("Microsoft.Maui.Controls.Core.XamlC.ListStringTypeConverter")]
	[Xaml.TypeConversion(typeof(List<string>))]
	public class ListStringTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value == null)
				return null;

			return value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
		}

		public override string ConvertToInvariantString(object value)
		{
			if (!(value is List<string> list))
				throw new NotSupportedException();
			return string.Join(", ", list);
		}
	}
}