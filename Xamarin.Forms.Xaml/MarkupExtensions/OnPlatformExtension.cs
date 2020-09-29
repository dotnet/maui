using System;
using System.Globalization;
using System.Reflection;

namespace Xamarin.Forms.Xaml
{
	[ContentProperty(nameof(Default))]
	public class OnPlatformExtension : IMarkupExtension
	{
		static object s_notset = new object();

		public object Default { get; set; } = s_notset;
		public object Android { get; set; } = s_notset;
		public object GTK { get; set; } = s_notset;
		public object iOS { get; set; } = s_notset;
		public object macOS { get; set; } = s_notset;
		public object Tizen { get; set; } = s_notset;
		public object UWP { get; set; } = s_notset;
		public object WPF { get; set; } = s_notset;

		public IValueConverter Converter { get; set; }

		public object ConverterParameter { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			if (Android == s_notset
				&& GTK == s_notset
				&& iOS == s_notset
				&& macOS == s_notset
				&& Tizen == s_notset
				&& UWP == s_notset
				&& WPF == s_notset
				&& Default == s_notset)
			{
				throw new XamlParseException("OnPlatformExtension requires a value to be specified for at least one platform or Default.", serviceProvider);
			}

			var valueProvider = serviceProvider?.GetService<IProvideValueTarget>() ?? throw new ArgumentException();

			BindableProperty bp;
			PropertyInfo pi = null;
			Type propertyType = null;

			if (valueProvider.TargetObject is Setter setter)
				bp = setter.Property;
			else
			{
				bp = valueProvider.TargetProperty as BindableProperty;
				pi = valueProvider.TargetProperty as PropertyInfo;
			}
			propertyType = bp?.ReturnType
						?? pi?.PropertyType
						?? throw new InvalidOperationException("Cannot determine property to provide the value for.");

			if (!TryGetValueForPlatform(out var value))
			{
				if (bp != null)
					return bp.GetDefaultValue(valueProvider.TargetObject as BindableObject);
				if (propertyType.GetTypeInfo().IsValueType)
					return Activator.CreateInstance(propertyType);
				return null;
			}

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
			var ret = value.ConvertTo(propertyType, () => pi, serviceProvider, out Exception exception);
			if (exception != null)
				throw exception;
			return ret;
		}

		bool TryGetValueForPlatform(out object value)
		{
			if (Device.RuntimePlatform == Device.Android && Android != s_notset)
			{
				value = Android;
				return true;
			}
			if (Device.RuntimePlatform == Device.GTK && GTK != s_notset)
			{
				value = GTK;
				return true;
			}
			if (Device.RuntimePlatform == Device.iOS && iOS != s_notset)
			{
				value = iOS;
				return true;
			}
			if (Device.RuntimePlatform == Device.macOS && macOS != s_notset)
			{
				value = macOS;
				return true;
			}
			if (Device.RuntimePlatform == Device.Tizen && Tizen != s_notset)
			{
				value = Tizen;
				return true;
			}
			if (Device.RuntimePlatform == Device.UWP && UWP != s_notset)
			{
				value = UWP;
				return true;
			}
			if (Device.RuntimePlatform == Device.WPF && WPF != s_notset)
			{
				value = WPF;
				return true;
			}
			value = Default;
			return value != s_notset;
		}
	}
}