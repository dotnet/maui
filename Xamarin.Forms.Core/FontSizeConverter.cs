using System;
using System.ComponentModel;
using System.Globalization;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms
{
	[TypeConversion(typeof(double))]
	public class FontSizeConverter : TypeConverter, IExtendedTypeConverter
	{
		[Obsolete("IExtendedTypeConverter.ConvertFrom is obsolete as of version 2.2.0. Please use ConvertFromInvariantString (string, IServiceProvider) instead.")]
		[EditorBrowsable(EditorBrowsableState.Never)]
		object IExtendedTypeConverter.ConvertFrom(CultureInfo culture, object value, IServiceProvider serviceProvider)
			=> ((IExtendedTypeConverter)this).ConvertFromInvariantString(value as string, serviceProvider);

		object IExtendedTypeConverter.ConvertFromInvariantString(string value, IServiceProvider serviceProvider)
		{
			if (value != null)
			{
				value = value.Trim();
				if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double size))
					return size;

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
			}
			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(double)));
		}

		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out double size))
					return size;
				value = value.Trim();

				if (value.Equals(nameof(NamedSize.Default), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Default, typeof(Label), false);
				if (value.Equals(nameof(NamedSize.Micro), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Micro, typeof(Label), false);
				if (value.Equals(nameof(NamedSize.Small), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Small, typeof(Label), false);
				if (value.Equals(nameof(NamedSize.Medium), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Medium, typeof(Label), false);
				if (value.Equals(nameof(NamedSize.Large), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Large, typeof(Label), false);
				if (value.Equals(nameof(NamedSize.Body), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Body, typeof(Label), false);
				if (value.Equals(nameof(NamedSize.Caption), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Caption, typeof(Label), false);
				if (value.Equals(nameof(NamedSize.Header), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Header, typeof(Label), false);
				if (value.Equals(nameof(NamedSize.Subtitle), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Subtitle, typeof(Label), false);
				if (value.Equals(nameof(NamedSize.Title), StringComparison.Ordinal))
					return Device.GetNamedSize(NamedSize.Title, typeof(Label), false);
				if (Enum.TryParse(value, out NamedSize namedSize))
					return Device.GetNamedSize(namedSize, typeof(Label), false);
			}
			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(double)));
		}
	}
}