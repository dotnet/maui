using System;
using System.Globalization;
using System.Reflection;

namespace System.Maui.Xaml
{
	[ContentProperty(nameof(Default))]
	public class OnAppThemeExtension : IMarkupExtension<BindingBase>
	{
		public OnAppThemeExtension()
		{
			ExperimentalFlags.VerifyFlagEnabled(nameof(OnAppThemeExtension), ExperimentalFlags.AppThemeExperimental, nameof(OnAppThemeExtension));

			Application.Current.RequestedThemeChanged += RequestedThemeChanged;
		}

		public object Default { get; set; }
		public object Light { get; set; }
		public object Dark { get; set; }
		public object Value	{ get; private set;	}

		public IValueConverter Converter { get; set; }
		public object ConverterParameter { get; set; }

		public object ProvideValue(IServiceProvider serviceProvider)
		{
			return (this as IMarkupExtension<BindingBase>).ProvideValue(serviceProvider);
		}

		object GetValue()
		{
			switch (Application.Current?.RequestedTheme)
			{
				default:
				case OSAppTheme.Light:
					return Light ?? Default;
				case OSAppTheme.Dark:
					return Dark ?? Default;
			}
		}

		void RequestedThemeChanged(object sender, AppThemeChangedEventArgs e)
		{
			Value = GetValue();
		}

		BindingBase IMarkupExtension<BindingBase>.ProvideValue(IServiceProvider serviceProvider)
		{
			if (Default == null
				&& Light == null
				&& Dark == null)
				throw new XamlParseException("OnAppThemeExtension requires a non-null value to be specified for at least one theme or Default.", serviceProvider);

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

			if (Converter != null)
			{
				var light = Converter.Convert(Light, propertyType, ConverterParameter, CultureInfo.CurrentUICulture);

				var dark = Converter.Convert(Dark, propertyType, ConverterParameter, CultureInfo.CurrentUICulture);
				var def = Converter.Convert(Dark, propertyType, ConverterParameter, CultureInfo.CurrentUICulture);

				return new OnAppTheme<object> { Light = light, Dark = dark, Default = def };
			}

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

				var light = converterProvider.Convert(Light, propertyType, minforetriever, serviceProvider);

				var dark = converterProvider.Convert(Dark, propertyType, minforetriever, serviceProvider);
				var def = converterProvider.Convert(Dark, propertyType, minforetriever, serviceProvider);

				return new OnAppTheme<object> { Light = light, Dark = dark, Default = def };
			}
			if (converterProvider != null)
			{
				var light = converterProvider.Convert(Light, propertyType, () => pi, serviceProvider);

				var dark = converterProvider.Convert(Dark, propertyType, () => pi, serviceProvider);
				var def = converterProvider.Convert(Default, propertyType, () => pi, serviceProvider);

				return new OnAppTheme<object> { Light = light, Dark = dark, Default = def };
			}

			var lightConverted = Light.ConvertTo(propertyType, () => pi, serviceProvider, out Exception lightException);
			
			if (lightException != null)
				throw lightException;

			var darkConverted = Dark.ConvertTo(propertyType, () => pi, serviceProvider, out Exception darkException);

			if (darkException != null)
				throw darkException;

			var defaultConverted = Dark.ConvertTo(propertyType, () => pi, serviceProvider, out Exception defaultException);

			if (defaultException != null)
				throw defaultException;

			return new OnAppTheme<object> { Light = Light, Dark = Dark, Default = defaultConverted };
		}
	}
}