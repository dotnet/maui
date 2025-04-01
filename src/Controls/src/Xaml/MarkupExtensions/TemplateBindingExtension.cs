using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Xaml
{
	[ContentProperty(nameof(Path))]
	[AcceptEmptyServiceProvider]
	[RequiresUnreferencedCode(TrimmerConstants.StringPathBindingWarning, Url = TrimmerConstants.ExpressionBasedBindingsDocsUrl)]
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

		[EditorBrowsable(EditorBrowsableState.Never)] public TypedBindingBase TypedBinding { get; set; }

		BindingBase IMarkupExtension<BindingBase>.ProvideValue(IServiceProvider serviceProvider)
		{
			if (TypedBinding is null)
			{
#pragma warning disable IL2026 // Using member 'Microsoft.Maui.Controls.Binding.Binding(String, BindingMode, IValueConverter, Object, String, Object)' which has 'RequiresUnreferencedCodeAttribute' can break functionality when trimming application code. Using bindings with string paths is not trim safe. Use expression-based binding instead.
				// This code is only reachable in XamlC compiled code when there is a missing x:DataType and the binding could not be compiled.
				// In that case, we produce a warning that the binding could not be compiled.
				return new Binding
				{
					Source = RelativeBindingSource.TemplatedParent,
					Path = Path,
					Mode = Mode,
					Converter = Converter,
					ConverterParameter = ConverterParameter,
					StringFormat = StringFormat
				};
#pragma warning restore IL2026
			}

			TypedBinding.Mode = Mode;
			TypedBinding.Converter = Converter;
			TypedBinding.ConverterParameter = ConverterParameter;
			TypedBinding.StringFormat = StringFormat;
			TypedBinding.Source = RelativeBindingSource.TemplatedParent;
			return TypedBinding;
		}

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
		{
			return (this as IMarkupExtension<BindingBase>).ProvideValue(serviceProvider);
		}
	}
}