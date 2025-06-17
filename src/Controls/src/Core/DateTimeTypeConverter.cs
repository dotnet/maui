#if NET6_0_OR_GREATER
using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls;
    
public class DateTimeTypeConverter : TypeConverter, IExtendedTypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type? sourceType)
        => sourceType == typeof(string) || sourceType == typeof(DateTime) || sourceType == typeof(DateOnly);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => destinationType == typeof(string) || destinationType == typeof(DateTime) || destinationType == typeof(DateOnly);

    object IExtendedTypeConverter.ConvertFromInvariantString(string value, IServiceProvider serviceProvider)
    {
        if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
        {
            return DateOnly.FromDateTime(result);
        }
        throw new NotSupportedException($"Cannot convert \"{value}\" into {typeof(DateOnly)}");
    }

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is DateTime dateTime)
        {
            return dateTime;
        }
		if (value is string stringValue && DateOnly.TryParse(stringValue, culture, DateTimeStyles.None, out DateOnly dateTimeOnly))
		{
			return dateTimeOnly;
		}
		if (value is string stringValue2 && DateTime.TryParse(stringValue2, culture, DateTimeStyles.None, out dateTime))
        {
            return dateTime;
        }
        throw new NotSupportedException($"Cannot convert \"{value}\" into {typeof(DateTime)}");
    }

    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type? destinationType)
    {
        if (value is DateOnly dateOnly)
        {
			return ConvertToDestinationType(dateOnly, destinationType, culture);
        }
		else if (value is DateTime dateTime)
        {
			return ConvertToDestinationType(dateTime, destinationType, culture);
        }
		else if (value is string stringValue)
		{
			if (DateTime.TryParse(stringValue, culture, DateTimeStyles.None, out dateTime))
			{
				return ConvertToDestinationType(dateTime, destinationType, culture);
			}
			else if (DateOnly.TryParse(stringValue, culture, out dateOnly))
			{
				return ConvertToDestinationType(dateOnly, destinationType, culture);
			}
		}

        throw new NotSupportedException($"Cannot convert \"{value}\" into {destinationType}");
    }

	private static object ConvertToDestinationType(DateOnly dateOnly, Type? destinationType, CultureInfo? culture)
	{
		if (destinationType == typeof(string))
		{
			return dateOnly.ToString(culture);
		}
		if (destinationType == typeof(DateTime))
		{
			return dateOnly.ToDateTime(TimeOnly.MinValue);
		}
		if (destinationType == typeof(DateOnly))
		{
			return dateOnly;
		}

		throw new NotSupportedException($"Cannot convert \"{dateOnly}\" into {destinationType}");
	}

	private static object ConvertToDestinationType(DateTime dateTime, Type? destinationType, CultureInfo? culture)
	{
		if (destinationType == typeof(string))
		{
			return dateTime.ToString(culture);
		}
		if (destinationType == typeof(DateTime))
		{
			return dateTime;
		}
		if (destinationType == typeof(DateOnly))
		{
			return DateOnly.FromDateTime(dateTime);
		}

		throw new NotSupportedException($"Cannot convert \"{dateTime}\" into {destinationType}");
	}
}
#endif