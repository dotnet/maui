using System;
using System.ComponentModel;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Xaml
{
	[ContentProperty(nameof(Path))]
	[AcceptEmptyServiceProvider]
	public sealed class BindingExtension : IMarkupExtension<BindingBase>
	{
		public string Path { get; set; } = Binding.SelfPath;
		public BindingMode Mode { get; set; } = BindingMode.Default;
		public IValueConverter Converter { get; set; }
		public object ConverterParameter { get; set; }
		public string StringFormat { get; set; }
		public object Source { get; set; }
		public string UpdateSourceEventName { get; set; }
		public object TargetNullValue { get; set; }
		public object FallbackValue { get; set; }
		[EditorBrowsable(EditorBrowsableState.Never)] public TypedBindingBase TypedBinding { get; set; }

		BindingBase IMarkupExtension<BindingBase>.ProvideValue(IServiceProvider serviceProvider)
		{
			if (TypedBinding == null)
			{
				if (RuntimeFeature.AreNonCompiledBindingsInXamlSupported)
				{
					return new Binding(Path, Mode, Converter, ConverterParameter, StringFormat, Source)
					{
						UpdateSourceEventName = UpdateSourceEventName,
						FallbackValue = FallbackValue,
						TargetNullValue = TargetNullValue,
					};
				}
				else
				{
					throw new InvalidOperationException(
						$"It was not possible to compile binding with path '{Path}'. Bindings with string paths are not supported. " +
						"Make sure all bindings in XAML are correctly annotated with x:DataType.");
				}
			}

			TypedBinding.Mode = Mode;
			TypedBinding.Converter = Converter;
			TypedBinding.ConverterParameter = ConverterParameter;
			TypedBinding.StringFormat = StringFormat;
			TypedBinding.Source = Source;
			TypedBinding.UpdateSourceEventName = UpdateSourceEventName;
			TypedBinding.FallbackValue = FallbackValue;
			TypedBinding.TargetNullValue = TargetNullValue;
			return TypedBinding;
		}

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
		{
			return (this as IMarkupExtension<BindingBase>).ProvideValue(serviceProvider);
		}
	}
}