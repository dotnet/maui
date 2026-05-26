using System;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Controls.Xaml
{
	/// <summary>
	/// Provides a XAML markup extension that creates a binding with different values for light and dark themes.
	/// </summary>
	[ContentProperty(nameof(Default))]
	[RequireService(
		[typeof(IProvideValueTarget),
		 typeof(IValueConverterProvider),
		 typeof(IXmlLineInfoProvider),
		 typeof(IConverterOptions)])]
	public class AppThemeBindingExtension : IMarkupExtension<BindingBase>
	{
		object _default;
		bool _hasdefault;
		object _light;
		bool _haslight;
		object _dark;
		bool _hasdark;

		/// <summary>
		/// Gets or sets the default value to use when no theme-specific value is set.
		/// </summary>
		public object Default
		{
			get => _default; set
			{
				_default = value;
				_hasdefault = true;
			}
		}

		/// <summary>
		/// Gets or sets the value to use when the light theme is active.
		/// </summary>
		public object Light
		{
			get => _light; set
			{
				_light = value;
				_haslight = true;
			}
		}

		/// <summary>
		/// Gets or sets the value to use when the dark theme is active.
		/// </summary>
		public object Dark
		{
			get => _dark; set
			{
				_dark = value;
				_hasdark = true;
			}
		}

		/// <summary>
		/// Gets the current value based on the active theme.
		/// </summary>
		public object Value { get; private set; }

		public object ProvideValue(IServiceProvider serviceProvider) => (this as IMarkupExtension<BindingBase>).ProvideValue(serviceProvider);

		BindingBase IMarkupExtension<BindingBase>.ProvideValue(IServiceProvider serviceProvider)
		{
			if (Default == null
				&& Light == null
				&& Dark == null)
				throw new XamlParseException("AppThemeBindingExtension requires a non-null value to be specified for at least one theme or Default.", serviceProvider);

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

			var converterProvider = serviceProvider?.GetService<IValueConverterProvider>();

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

			var binding = new AppThemeBinding();
			if (converterProvider != null)
			{
				if (_haslight)
					binding.Light = converterProvider.Convert(Light, propertyType, minforetriever, serviceProvider);
				if (_hasdark)
					binding.Dark = converterProvider.Convert(Dark, propertyType, minforetriever, serviceProvider);
				if (_hasdefault)
					binding.Default = converterProvider.Convert(Default, propertyType, minforetriever, serviceProvider);
				return binding;
			}

			Exception converterException = null;

			if (converterException != null && _haslight)
				binding.Light = Light.ConvertTo(propertyType, minforetriever, serviceProvider, out converterException);
			if (converterException != null && _hasdark)
				binding.Dark = Dark.ConvertTo(propertyType, minforetriever, serviceProvider, out converterException);
			if (converterException != null && _hasdefault)
				binding.Default = Default.ConvertTo(propertyType, minforetriever, serviceProvider, out converterException);

			if (converterException != null)
				throw converterException;

			return binding;
		}
	}
}