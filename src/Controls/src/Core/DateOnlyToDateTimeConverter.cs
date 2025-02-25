#if NET6_0_OR_GREATER
using System;
using System.ComponentModel;
using System.Globalization;

namespace Microsoft.Maui.Controls
{
    internal class DateOnlyToDateTimeConverter : TypeConverter, IExtendedTypeConverter
    {
        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
            => sourceType == typeof(string) || sourceType == typeof(DateTime);

        public override bool CanConvertTo(ITypeDescriptorContext? context, Type? destinationType)
            => destinationType == typeof(string) || destinationType == typeof(DateTime);

        object IExtendedTypeConverter.ConvertFromInvariantString(string value, IServiceProvider serviceProvider)
        {
            if (DateTime.TryParse(value, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return DateOnly.FromDateTime(result);
            }
            throw new NotSupportedException($"Cannot convert \"{value}\" into {typeof(DateOnly)}");
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, CultureInfo? culture, object? value)
        {
            if (value is DateTime dateTime)
            {
                return DateOnly.FromDateTime(dateTime);
            }
            if (value is string stringValue && DateTime.TryParse(stringValue, CultureInfo.InvariantCulture, DateTimeStyles.None, out var result))
            {
                return DateOnly.FromDateTime(result);
            }
            throw new NotSupportedException($"Cannot convert \"{value}\" into {typeof(DateOnly)}");
        }

        public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
        {
            if (value is DateOnly dateOnly)
            {
                if (destinationType == typeof(string))
                {
                    return dateOnly.ToString(culture);
                }
                if (destinationType == typeof(DateTime))
                {
                    return dateOnly.ToDateTime(TimeOnly.MinValue);
                }
            }
            throw new NotSupportedException($"Cannot convert \"{value}\" into {destinationType}");
        }
    }
}
#endif