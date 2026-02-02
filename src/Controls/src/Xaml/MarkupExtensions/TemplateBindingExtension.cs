using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Xaml
{
	/// <summary>
	/// Provides a XAML markup extension that creates a binding to the templated parent.
	/// </summary>
	[ContentProperty(nameof(Path))]
	[AcceptEmptyServiceProvider]
	[RequiresUnreferencedCode(TrimmerConstants.StringPathBindingWarning, Url = TrimmerConstants.ExpressionBasedBindingsDocsUrl)]
	public sealed class TemplateBindingExtension : IMarkupExtension<BindingBase>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="TemplateBindingExtension"/> class.
		/// </summary>
		public TemplateBindingExtension()
		{
			Mode = BindingMode.Default;
			Path = Binding.SelfPath;
		}

		/// <summary>
		/// Gets or sets the path to the binding source property on the templated parent.
		/// </summary>
		public string Path { get; set; }

		/// <summary>
		/// Gets or sets the binding mode.
		/// </summary>
		public BindingMode Mode { get; set; }

		/// <summary>
		/// Gets or sets the converter to use when converting between source and target values.
		/// </summary>
		public IValueConverter Converter { get; set; }

		/// <summary>
		/// Gets or sets a parameter to pass to the converter.
		/// </summary>
		public object ConverterParameter { get; set; }

		/// <summary>
		/// Gets or sets a format string to use when converting the bound value to a string.
		/// </summary>
		public string StringFormat { get; set; }

		/// <summary>For internal use only. This API can be changed or removed without notice at any time.</summary>
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