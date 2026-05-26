
using System;
using System.Globalization;

namespace Microsoft.Maui
{
	internal static class PrimitiveTypeConversions
	{
		internal static bool IsImplicitlyConvertibleToDouble(Type type)
			=> type == typeof(double)
				|| type == typeof(float)
				|| type == typeof(int)
				|| type == typeof(uint)
				|| type == typeof(long)
				|| type == typeof(ulong)
				|| type == typeof(short)
				|| type == typeof(ushort)
				|| type == typeof(byte)
				|| type == typeof(sbyte)
				|| type == typeof(char);

		internal static bool TryGetDouble(object? value, out double result)
		{
			if (value is not null && IsImplicitlyConvertibleToDouble(value.GetType()))
			{
				result = Convert.ToDouble(value, CultureInfo.InvariantCulture);
				return true;
			}

			result = 0;
			return false;
		}
	}
}
