using System;
using System.Globalization;
using System.Reflection;

namespace Xamarin.Forms.Xaml
{
	[ContentProperty("Default")]
	public class OnIdiomExtension : IMarkupExtension
	{
		// See Device.Idiom

		public object Default { get; set; }
		public object Phone { get; set; }
		public object Tablet { get; set; }
		public object Desktop { get; set; }
		public object TV { get; set; }
		public object Watch { get; set; }

		public IValueConverter Converter { get; set; }

		public object ConverterParameter { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			var lineInfo = serviceProvider?.GetService<IXmlLineInfoProvider>()?.XmlLineInfo;
			if (Default == null && Phone == null &&
				Tablet == null && Desktop == null && TV == null && Watch == null)
			{
				throw new XamlParseException("OnIdiomExtension requires a non-null value to be specified for at least one idiom or Default.", lineInfo ?? new XmlLineInfo());
			}

			var valueProvider = serviceProvider?.GetService<IProvideValueTarget>() ?? throw new ArgumentException();

			var bp = valueProvider.TargetProperty as BindableProperty;
			var pi = valueProvider.TargetProperty as PropertyInfo;
			var propertyType = bp?.ReturnType
				?? pi?.PropertyType
				?? throw new InvalidOperationException("Cannot determine property to provide the value for.");

			var value = GetValue();
			var info = propertyType.GetTypeInfo();
			if (value == null && info.IsValueType)
				return Activator.CreateInstance(propertyType);

			if (Converter != null)
				return Converter.Convert(value, propertyType, ConverterParameter, CultureInfo.CurrentUICulture);

			var converterProvider = serviceProvider?.GetService<IValueConverterProvider>();

			if (converterProvider != null)
				return converterProvider.Convert(value, propertyType, () => pi, serviceProvider);
			else
				return value.ConvertTo(propertyType, () => pi, serviceProvider);
		}

		object GetValue()
		{
			switch (Device.Idiom)
			{
				case TargetIdiom.Phone:
					return Phone ?? Default;
				case TargetIdiom.Tablet:
					return Tablet ?? Default;
				case TargetIdiom.Desktop:
					return Desktop ?? Default;
				case TargetIdiom.TV:
					return TV ?? Default;
				case TargetIdiom.Watch:
					return Watch ?? Default;
				default:
					return Default;
			}
		}
	}
}
