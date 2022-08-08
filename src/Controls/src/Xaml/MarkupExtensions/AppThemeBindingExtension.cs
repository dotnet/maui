using System;
using System.Globalization;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Maui.Controls.Xaml
{
	[ContentProperty(nameof(Default))]
	public class AppThemeBindingExtension : IMarkupExtension<BindingBase>
	{
		object _default;
		bool _hasdefault;
		object _light;
		bool _haslight;
		object _dark;
		bool _hasdark;

		public object Default
		{
			get => _default; set
			{
				_default = value;
				_hasdefault = true;
			}
		}
		public object Light
		{
			get => _light; set
			{
				_light = value;
				_haslight = true;
			}
		}
		public object Dark
		{
			get => _dark; set
			{
				_dark = value;
				_hasdark = true;
			}
		}
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
			Type propertyType = null;

			if (valueProvider.TargetObject is Setter setter)
				bp = setter.Property;
			else
				bp = valueProvider.TargetProperty as BindableProperty;

			var binding = new AppThemeBinding();

			Exception converterException = null;
			if (converterException == null && _haslight)
				binding.Light = Light.ConvertTo(propertyType, bp.GetBindablePropertyTypeConverter, serviceProvider, out converterException);
			if (converterException == null && _hasdark)
				binding.Dark = Dark.ConvertTo(propertyType, bp.GetBindablePropertyTypeConverter, serviceProvider, out converterException);
			if (converterException == null && _hasdefault)
				binding.Default = Default.ConvertTo(propertyType, bp.GetBindablePropertyTypeConverter, serviceProvider, out converterException);

			if (converterException != null)
			{
				var lineInfo = (serviceProvider.GetService(typeof(IXmlLineInfoProvider)) is IXmlLineInfoProvider lineInfoProvider) ? lineInfoProvider.XmlLineInfo : new XmlLineInfo();
				throw new XamlParseException(converterException.Message, serviceProvider, converterException);
			}

			return binding;
		}
	}
}