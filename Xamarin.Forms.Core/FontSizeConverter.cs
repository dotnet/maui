using System;
using System.Globalization;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms
{
	[Xaml.TypeConversion(typeof(double))]
	public class FontSizeConverter : TypeConverter, IExtendedTypeConverter
	{
		[Obsolete("IExtendedTypeConverter.ConvertFrom is obsolete as of version 2.2.0. Please use ConvertFromInvariantString (string, IServiceProvider) instead.")]
		object IExtendedTypeConverter.ConvertFrom(CultureInfo culture, object value, IServiceProvider serviceProvider)
		{
			return ((IExtendedTypeConverter)this).ConvertFromInvariantString(value as string, serviceProvider);
		}

		object IExtendedTypeConverter.ConvertFromInvariantString(string value, IServiceProvider serviceProvider)
		{
			if (value != null)
			{
				double size;
				if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out size))
					return size;
				var ignoreCase = (serviceProvider?.GetService(typeof(IConverterOptions)) as IConverterOptions)?.IgnoreCase ?? false;
				NamedSize namedSize;
				if (Enum.TryParse(value, ignoreCase, out namedSize))
				{
					Type type;
					var valueTargetProvider = serviceProvider.GetService(typeof(IProvideValueTarget)) as IProvideValueTarget;
					type = valueTargetProvider != null ? valueTargetProvider.TargetObject.GetType() : typeof(Label);
					return Device.GetNamedSize(namedSize, type, false);
				}
			}
			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(double)));
		}

		public override object ConvertFromInvariantString(string value)
		{
			if (value != null)
			{
				double size;
				if (double.TryParse(value, NumberStyles.Number, CultureInfo.InvariantCulture, out size))
					return size;
				NamedSize namedSize;
				if (Enum.TryParse(value, out namedSize))
					return Device.GetNamedSize(namedSize, typeof(Label), false);
			}
			throw new InvalidOperationException(string.Format("Cannot convert \"{0}\" into {1}", value, typeof(double)));
		}
	}
}