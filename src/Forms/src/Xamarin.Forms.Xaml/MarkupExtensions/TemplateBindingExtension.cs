using System;

namespace Xamarin.Forms.Xaml
{
	[ContentProperty(nameof(Path))]
	[AcceptEmptyServiceProvider]
	public sealed class TemplateBindingExtension : IMarkupExtension<BindingBase>
	{
		public TemplateBindingExtension()
		{
			Mode = BindingMode.Default;
			Path = Binding.SelfPath;
		}

		public string Path { get; set; }

		public BindingMode Mode { get; set; }

		public IValueConverter Converter { get; set; }

		public object ConverterParameter { get; set; }

		public string StringFormat { get; set; }

		BindingBase IMarkupExtension<BindingBase>.ProvideValue(IServiceProvider serviceProvider)
		{
			return new Binding
			{
				Source = RelativeBindingSource.TemplatedParent,
				Path = Path,
				Mode = Mode,
				Converter = Converter,
				ConverterParameter = ConverterParameter,
				StringFormat = StringFormat
			};
		}

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
		{
			return (this as IMarkupExtension<BindingBase>).ProvideValue(serviceProvider);
		}
	}
}