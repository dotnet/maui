using System;
using System.Collections.Generic;
using System.Linq;

namespace Xamarin.Forms
{
	[Xaml.ProvideCompiled("Xamarin.Forms.Core.XamlC.ListStringTypeConverter")]
	[Xaml.TypeConversion(typeof(List<string>))]
	public class ListStringTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value == null)
				return null;

			return value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
		}
	}
}