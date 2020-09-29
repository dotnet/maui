using System;
using System.Globalization;
using System.Reflection;
using System.Xml;

namespace Xamarin.Forms.Xaml
{
	[ContentProperty(nameof(Default))]
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
			if (Default == null
				&& Phone == null
				&& Tablet == null
				&& Desktop == null
				&& TV == null
				&& Watch == null)
				throw new XamlParseException("OnIdiomExtension requires a non-null value to be specified for at least one idiom or Default.", serviceProvider);

			var valueProvider = serviceProvider?.GetService<IProvideValueTarget>() ?? throw new ArgumentException();

			BindableProperty bp;
			PropertyInfo pi = null;
			Type propertyType = null;

			if (valueProvider.TargetObject is Setter setter)
			{
				bp = setter.Property;
			}
			else
			{
				bp = valueProvider.TargetProperty as BindableProperty;
				pi = valueProvider.TargetProperty as PropertyInfo;
			}
			propertyType = bp?.ReturnType
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
			{
				MemberInfo minforetriever()
				{
					if (pi != null)
						return pi;

					MemberInfo minfo = null;
					try
					{
						minfo = bp.DeclaringType.GetRuntimeProperty(bp.PropertyName);
					}
					catch (AmbiguousMatchException e)
					{
						throw new XamlParseException($"Multiple properties with name '{bp.DeclaringType}.{bp.PropertyName}' found.", serviceProvider, innerException: e);
					}
					if (minfo != null)
						return minfo;
					try
					{
						return bp.DeclaringType.GetRuntimeMethod("Get" + bp.PropertyName, new[] { typeof(BindableObject) });
					}
					catch (AmbiguousMatchException e)
					{
						throw new XamlParseException($"Multiple methods with name '{bp.DeclaringType}.Get{bp.PropertyName}' found.", serviceProvider, innerException: e);
					}
				}

				return converterProvider.Convert(value, propertyType, minforetriever, serviceProvider);
			}
			if (converterProvider != null)
				return converterProvider.Convert(value, propertyType, () => pi, serviceProvider);

			var ret = value.ConvertTo(propertyType, () => pi, serviceProvider, out Exception exception);
			if (exception != null)
				throw exception;
			return ret;
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