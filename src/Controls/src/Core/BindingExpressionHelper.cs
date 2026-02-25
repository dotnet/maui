using System;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Microsoft.Maui.Controls
{
	internal static class BindingExpressionHelper
	{
		static readonly Type[] DecimalTypes = { typeof(float), typeof(decimal), typeof(double) };

		internal static bool TryConvert(ref object value, BindableProperty targetProperty, Type convertTo, bool toTarget)
		{
			if (value == null)
				return !convertTo.GetTypeInfo().IsValueType || Nullable.GetUnderlyingType(convertTo) != null;
			try
			{
				if ((toTarget && targetProperty.TryConvert(ref value)) || (!toTarget && convertTo.IsInstanceOfType(value)))
					return true;
			}
			catch (InvalidOperationException)
			{ //that's what TypeConverters ususally throw
				return false;
			}

			object original = value;
			try
			{
				var underlyingType = Nullable.GetUnderlyingType(convertTo);
				var stringValue = value as string ?? string.Empty;

				// Handle empty string conversion to nullable types
				// Empty string should convert to null for nullable value types
				// Only apply to actual string values to avoid converting non-string inputs
				// See: https://github.com/dotnet/maui/issues/8342
				if (underlyingType != null && value is string && string.IsNullOrEmpty(stringValue))
				{
					value = null!;
					return true;
				}

				convertTo = underlyingType ?? convertTo;
				// see: https://bugzilla.xamarin.com/show_bug.cgi?id=32871
				// do not canonicalize "*.[.]"; "1." should not update bound BindableProperty
				if (stringValue.EndsWith(CultureInfo.CurrentCulture.NumberFormat.NumberDecimalSeparator, StringComparison.Ordinal) && DecimalTypes.Contains(convertTo))
				{
					value = original;
					return false;
				}

				// do not canonicalize "-0"; user will likely enter a period after "-0"
				if (stringValue == "-0" && DecimalTypes.Contains(convertTo))
				{
					value = original;
					return false;
				}

				value = Convert.ChangeType(value, convertTo, CultureInfo.CurrentCulture);

				return true;
			}
			catch (Exception ex) when (ex is InvalidCastException || ex is FormatException || ex is InvalidOperationException || ex is OverflowException)
			{
				value = original;
				return false;
			}
		}
	}
}
