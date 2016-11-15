using System;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Xaml
{
	[ContentProperty("Path")]
	public sealed class BindingExtension : IMarkupExtension<BindingBase>
	{
		public BindingExtension()
		{
			Mode = BindingMode.Default;
			Path = Binding.SelfPath;
		}

		public string Path { get; set; }

		public BindingMode Mode { get; set; }

		public IValueConverter Converter { get; set; }

		public object ConverterParameter { get; set; }

		public string StringFormat { get; set; }

		public object Source { get; set; }

		public string UpdateSourceEventName { get; set; }

		public TypedBindingBase TypedBinding { get; set; }

		BindingBase IMarkupExtension<BindingBase>.ProvideValue(IServiceProvider serviceProvider)
		{
			if (TypedBinding == null)
				return new Binding(Path, Mode, Converter, ConverterParameter, StringFormat, Source) { UpdateSourceEventName = UpdateSourceEventName };

			TypedBinding.Mode = Mode;
			TypedBinding.Converter = Converter;
			TypedBinding.ConverterParameter = ConverterParameter;
			TypedBinding.StringFormat = StringFormat;
			TypedBinding.Source = Source;
			TypedBinding.UpdateSourceEventName = UpdateSourceEventName;
			return TypedBinding;
		}

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
		{
			return (this as IMarkupExtension<BindingBase>).ProvideValue(serviceProvider);
		}
	}
}