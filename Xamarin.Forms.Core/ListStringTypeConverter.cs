using System;
using System.Linq;

namespace Xamarin.Forms
{
	public class ListStringTypeConverter : TypeConverter
	{
		public override object ConvertFromInvariantString(string value)
		{
			if (value == null)
				return null;
			
			return value.Split(new [] { ',' }, StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();
		}
	}
}