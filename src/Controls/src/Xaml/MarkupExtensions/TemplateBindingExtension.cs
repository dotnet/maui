using System;

namespace Microsoft.Maui.Controls.Xaml
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
			if (RuntimeFeature.AreNonCompiledBindingsInXamlSupported)
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
			else
			{
				throw new InvalidOperationException(
					$"It was not possible to compile binding with path '{Path}'. Bindings with string paths are not supported. " +
					"Use {Binding RelativeSource={RelativeSource TemplatedParent}} instead of {TemplateBinding} Make sure all bindings in XAML are correctly annotated with x:DataType.");
			}
		}

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
		{
			return (this as IMarkupExtension<BindingBase>).ProvideValue(serviceProvider);
		}
	}
}