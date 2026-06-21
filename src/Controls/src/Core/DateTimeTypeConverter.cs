#if NET6_0_OR_GREATER
using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls;

internal class DateTimeTypeConverter : TypeConverter
{
    public override bool CanConvertFrom(ITypeDescriptorContext? context, Type? sourceType)
        => sourceType == typeof(DateOnly) || sourceType == typeof(string);

    public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
        => destinationType == typeof(DateTime);

    public override object ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object value)
    {
        if (value is DateTime dateTime)
        {
            return dateTime;
        }
        if (value is DateOnly dateOnly)
        {
            return dateOnly.ToDateTime(TimeOnly.MinValue);
        }
        if (value is string stringValue)
        {
            if (DateOnly.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateOnly dateTimeOnly))
            {
                return dateTimeOnly.ToDateTime(TimeOnly.MinValue);
            }
            else if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                return dateTime;
            }
        }

        throw new NotImplementedException($"Cannot convert \"{value}\" into {typeof(DateTime)}");
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
            if (DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out dateTime))
            {
                return ConvertToDestinationType(dateTime, destinationType, culture);
            }
            else if (DateOnly.TryParse(stringValue, CultureInfo.InvariantCulture, out dateOnly))
            {
                return ConvertToDestinationType(dateOnly, destinationType, culture);
            }
        }

        throw new NotImplementedException($"Cannot convert \"{value}\" into {destinationType}");
    }

    static object ConvertToDestinationType(DateOnly dateOnly, Type? destinationType, CultureInfo? culture)
    {
        if (destinationType == typeof(string))
        {
            return dateOnly.ToString( CultureInfo.InvariantCulture);
        }
        else if (destinationType == typeof(DateTime))
        {
            return dateOnly.ToDateTime(TimeOnly.MinValue);
        }
        else if (destinationType == typeof(DateOnly))
        {
            return dateOnly;
        }

        throw new NotImplementedException($"Cannot convert \"{dateOnly}\" into {destinationType}");
    }

    static object ConvertToDestinationType(DateTime dateTime, Type? destinationType, CultureInfo? culture)
    {
        if (destinationType == typeof(string))
        {
            return dateTime.ToString(CultureInfo.InvariantCulture);
        }
        else if (destinationType == typeof(DateTime))
        {
            return dateTime;
        }
        else if (destinationType == typeof(DateOnly))
        {
            return DateOnly.FromDateTime(dateTime);
        }

        throw new NotImplementedException($"Cannot convert \"{dateTime}\" into {destinationType}");
    }
}

#endif