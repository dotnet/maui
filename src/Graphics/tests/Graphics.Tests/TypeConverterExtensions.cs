using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Microsoft.Maui.Graphics.Tests
{
	public static class TypeConverterExtensions
	{
		public static bool TryConvertFrom<TConverterType, T>(this string input, out T result)
			where TConverterType : TypeConverter, new()
		{
			var converter = new TConverterType();

			try
			{
				result = (T)converter.ConvertFrom(input);
				return true;
			}
			catch { }

			result = default;
			return false;
		}
	}
}
