#if NET6_0_OR_GREATER
using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls;

[ProvideCompiled("Microsoft.Maui.Controls.XamlC.TimeSpanTypeConverter")]
public class TimeSpanTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type? sourceType)
        => sourceType == typeof(string) || sourceType == typeof(TimeOnly) || sourceType == typeof(TimeSpan);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => destinationType == typeof(string) || destinationType == typeof(TimeSpan) || destinationType == typeof(TimeOnly);

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
        else if (value is string stringValue && TimeOnly.TryParse(stringValue, out var result))
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
            if (TimeOnly.TryParse(stringValue, culture, out timeOnly))
            {
                return ConvertToDestinationType(timeOnly, destinationType, culture);
            }
            else if (TimeSpan.TryParse(stringValue, culture, out var timeSpan1))
            {
                return ConvertToDestinationType(timeSpan1, destinationType, culture);
            }
        }

        throw new NotSupportedException($"Cannot convert \"{value}\" into {destinationType}");
    }

    private static object ConvertToDestinationType(TimeOnly timeOnly, Type? destinationType, CultureInfo? culture)
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

        throw new NotSupportedException($"Cannot convert \"{timeOnly}\" into {destinationType}");
    }

    private static object ConvertToDestinationType(TimeSpan timeSpan, Type? destinationType, CultureInfo? culture)
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

        throw new NotSupportedException($"Cannot convert \"{timeSpan}\" into {destinationType}");
    }
}
#endif