#if NET6_0_OR_GREATER
using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls;

internal class TimeSpanTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type? sourceType)
        => sourceType == typeof(TimeOnly);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => destinationType == typeof(TimeSpan);

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is TimeSpan timeSpan)
        {
            return timeSpan;
        }
        else if (value is TimeOnly timeOnly)
        {
            return timeOnly.ToTimeSpan();
        }
        else if (value is string stringValue)
        {
            if (TimeOnly.TryParse(stringValue, culture, out var timeOnlyResult))
            {
                return timeOnlyResult.ToTimeSpan();
            }
            else if (TimeSpan.TryParse(stringValue, culture, out var timeSpanResult))
            {
                return timeSpanResult;
            }
        }
        throw new NotImplementedException($"Cannot convert \"{value}\" into {typeof(TimeSpan)}");
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
            if (TimeOnly.TryParse(stringValue, culture, out timeOnly))
            {
                return ConvertToDestinationType(timeOnly, destinationType, culture);
            }
            else if (TimeSpan.TryParse(stringValue, culture, out var timeSpan1))
            {
                return ConvertToDestinationType(timeSpan1, destinationType, culture);
            }
        }

        throw new NotImplementedException($"Cannot convert \"{value}\" into {destinationType}");
    }

    static object ConvertToDestinationType(TimeOnly timeOnly, Type? destinationType, CultureInfo? culture)
    {
        if (destinationType == typeof(string))
        {
            return timeOnly.ToString(culture);
        }
        else if (destinationType == typeof(TimeSpan))
        {
            return timeOnly.ToTimeSpan();
        }
        else if (destinationType == typeof(TimeOnly))
        {
            return timeOnly;
        }

        throw new NotImplementedException($"Cannot convert \"{timeOnly}\" into {destinationType}");
    }

    static object ConvertToDestinationType(TimeSpan timeSpan, Type? destinationType, CultureInfo? culture)
    {
        if (destinationType == typeof(string))
        {
            return timeSpan.ToString();
        }
        else if (destinationType == typeof(TimeSpan))
        {
            return timeSpan;
        }
        else if (destinationType == typeof(TimeOnly))
        {
            return TimeOnly.FromTimeSpan(timeSpan);
        }

        throw new NotImplementedException($"Cannot convert \"{timeSpan}\" into {destinationType}");
    }
}
#endif