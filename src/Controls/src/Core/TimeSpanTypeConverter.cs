#if NET6_0_OR_GREATER
using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls;

public class TimeSpanTypeConverter : TypeConverter, IExtendedTypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type? sourceType)
        => sourceType == typeof(string) || sourceType == typeof(TimeOnly) || sourceType == typeof(TimeSpan);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => destinationType == typeof(string) || destinationType == typeof(TimeSpan) || destinationType == typeof(TimeOnly);

    object IExtendedTypeConverter.ConvertFromInvariantString(string value, IServiceProvider serviceProvider)
    {
        if (TimeOnly.TryParse(value, out var result))
        {
            return result.ToTimeSpan();
        }
        throw new NotSupportedException($"Cannot convert \"{value}\" into {typeof(TimeSpan)}");
    }

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is TimeOnly timeOnly)
        {
            return timeOnly.ToTimeSpan();
        }
        if (value is string stringValue && TimeOnly.TryParse(stringValue, out var result))
        {
            return result.ToTimeSpan();
        }
        throw new NotSupportedException($"Cannot convert \"{value}\" into {typeof(TimeSpan)}");
    }

    public override object ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type? destinationType)
    {
        if (value is TimeSpan timeSpan)
        {
            return ConvertToDestinationType(timeSpan, destinationType, culture);
        }
		else if (value is TimeOnly timeOnly)
		{
			return ConvertToDestinationType(timeOnly, destinationType, culture);
		}
		else if (value is string stringValue)
		{
			if (destinationType == typeof(string))
			{
				return stringValue;
			}
			if (TimeOnly.TryParse(stringValue, culture, out timeOnly))
			{
				return ConvertToDestinationType(timeOnly, destinationType, culture);
			}
			else if (TimeSpan.TryParse(stringValue, culture, out var timeSpan))
			{
				return ConvertToDestinationType(timeSpan, destinationType, culture);
			}
		}

        throw new NotSupportedException($"Cannot convert \"{value}\" into {destinationType}");
    }

	private static object ConvertToDestinationType(TimeOnly timeOnly, Type destinationType, CultureInfo? culture)
	{
		if (destinationType == typeof(string))
		{
			return timeOnly.ToString(culture);
		}
		if (destinationType == typeof(TimeSpan))
		{
			return timeOnly.ToTimeSpan();
		}
		if (destinationType == typeof(TimeOnly))
		{
			return timeOnly;
		}

		throw new NotSupportedException($"Cannot convert \"{value}\" into {destinationType}");
	}

	private static object ConvertToDestinationType(TimeSpan timeSpan, Type destinationType, CultureInfo? culture)
	{
		if (destinationType == typeof(string))
		{
			return timeSpan.ToString(culture);
		}
		if (destinationType == typeof(TimeSpan))
		{
			return timeSpan;
		}
		if (destinationType == typeof(TimeOnly))
		{
			return TimeOnly.FromTimeSpan(timeSpan);
		}

		throw new NotSupportedException($"Cannot convert \"{value}\" into {destinationType}");
	}
}
#endif