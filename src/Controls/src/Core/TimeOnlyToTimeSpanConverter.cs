#if NET6_0_OR_GREATER
using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
    internal class TimeOnlyToTimeSpanConverter : TypeConverter, IExtendedTypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            => sourceType == typeof(string) || sourceType == typeof(TimeOnly);

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
            => destinationType == typeof(string) || destinationType == typeof(TimeSpan);

        object IExtendedTypeConverter.ConvertFromInvariantString(string value, IServiceProvider serviceProvider)
        {
            if (TimeOnly.TryParse(value, out var result))
            {
                return result.ToTimeSpan();
            }
            throw new NotSupportedException($"Cannot convert \"{value}\" into {typeof(TimeSpan)}");
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
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

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is TimeSpan timeSpan)
            {
                if (destinationType == typeof(string))
                {
                    return timeSpan.ToString();
                }
                if (destinationType == typeof(TimeOnly))
                {
                    return TimeOnly.FromTimeSpan(timeSpan);
                }
            }
            throw new NotSupportedException($"Cannot convert \"{value}\" into {destinationType}");
        }
    }
}
#endif