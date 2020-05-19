using System;
using Xamarin.Forms.Xaml;

namespace Xamarin.Forms.Pages
{
	[ContentProperty("Path")]
	[AcceptEmptyServiceProvider]
	public sealed class DataSourceBindingExtension : IMarkupExtension<BindingBase>
	{
		public DataSourceBindingExtension()
		{
			Mode = BindingMode.Default;
			Path = Binding.SelfPath;
		}

		public IValueConverter Converter { get; set; }

		public object ConverterParameter { get; set; }

		public BindingMode Mode { get; set; }

		public string Path { get; set; }

		public string StringFormat { get; set; }

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
		{
			return (this as IMarkupExtension<BindingBase>).ProvideValue(serviceProvider);
		}

		BindingBase IMarkupExtension<BindingBase>.ProvideValue(IServiceProvider serviceProvider)
		{
			return new DataSourceBinding(Path, Mode, Converter, ConverterParameter, StringFormat);
		}
	}
}