#nullable disable
using System;
using System.ComponentModel;
using System.Globalization;
using Microsoft.Maui.Controls.Xaml;

namespace Microsoft.Maui.Controls
{
	/// <summary>Converts a string into a font size.</summary>
	[ProvideCompiled("Microsoft.Maui.Controls.XamlC.FontSizeTypeConverter")]
	public class FontSizeConverter : TypeConverter, IExtendedTypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> sourceType == typeof(string);

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> destinationType == typeof(string);

		object IExtendedTypeConverter.ConvertFromInvariantString(string value, IServiceProvider serviceProvider)
		{
			if (value != null)
			{
				value = value.Trim();
				if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double size))
					return size;

#pragma warning disable CS0612 // Type or member is obsolete
				var ignoreCase = (serviceProvider?.GetService(typeof(IConverterOptions)) as IConverterOptions)?.IgnoreCase ?? false;
				var sc = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
				NamedSize? namedSize = null;

				if (value.Equals(nameof(NamedSize.Default), sc))
					namedSize = NamedSize.Default;
				else if (value.Equals(nameof(NamedSize.Micro), sc))
					namedSize = NamedSize.Micro;
				else if (value.Equals(nameof(NamedSize.Small), sc))
					namedSize = NamedSize.Small;
				else if (value.Equals(nameof(NamedSize.Medium), sc))
					namedSize = NamedSize.Medium;
				else if (value.Equals(nameof(NamedSize.Large), sc))
					namedSize = NamedSize.Large;
				else if (value.Equals(nameof(NamedSize.Body), sc))
					namedSize = NamedSize.Body;
				else if (value.Equals(nameof(NamedSize.Caption), sc))
					namedSize = NamedSize.Caption;
				else if (value.Equals(nameof(NamedSize.Header), sc))
					namedSize = NamedSize.Header;
				else if (value.Equals(nameof(NamedSize.Subtitle), sc))
					namedSize = NamedSize.Subtitle;
				else if (value.Equals(nameof(NamedSize.Title), sc))
					namedSize = NamedSize.Title;
				else if (Enum.TryParse(value, ignoreCase, out NamedSize ns))
					namedSize = ns;

				if (namedSize.HasValue)
				{
					var type = serviceProvider.GetService(typeof(IProvideValueTarget)) is IProvideValueTarget valueTargetProvider ? valueTargetProvider.TargetObject.GetType() : typeof(Label);
					return Device.GetNamedSize(namedSize.Value, type, false);
				}
#pragma warning restore CS0612 // Type or member is obsolete
			}
			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(double)));
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			var strValue = value?.ToString();

			if (strValue != null)
			{
				if (double.TryParse(strValue, NumberStyles.Number, CultureInfo.InvariantCulture, out double size))
					return size;
				strValue = strValue.Trim();

#pragma warning disable CS0612 // Type or member is obsolete
				if (strValue.Equals(nameof(NamedSize.Default), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Default, typeof(Label), false);
				if (strValue.Equals(nameof(NamedSize.Micro), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Micro, typeof(Label), false);
				if (strValue.Equals(nameof(NamedSize.Small), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Small, typeof(Label), false);
				if (strValue.Equals(nameof(NamedSize.Medium), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Medium, typeof(Label), false);
				if (strValue.Equals(nameof(NamedSize.Large), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Large, typeof(Label), false);
				if (strValue.Equals(nameof(NamedSize.Body), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Body, typeof(Label), false);
				if (strValue.Equals(nameof(NamedSize.Caption), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Caption, typeof(Label), false);
				if (strValue.Equals(nameof(NamedSize.Header), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Header, typeof(Label), false);
				if (strValue.Equals(nameof(NamedSize.Subtitle), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Subtitle, typeof(Label), false);
				if (strValue.Equals(nameof(NamedSize.Title), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Title, typeof(Label), false);
				if (Enum.TryParse(strValue, out NamedSize namedSize))
					return Device.GetNamedSize(namedSize, typeof(Label), false);
#pragma warning restore CS0612 // Type or member is obsolete
			}
			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", strValue, typeof(double)));
		}

		public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
		{
			if (value is not double d)
				throw new NotSupportedException();
			return $"{d.ToString(CultureInfo.InvariantCulture)}";
		}
	}
}
