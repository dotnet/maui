using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Xaml
{
	/// <summary>
	/// Provides a XAML markup extension that creates a <see cref="Binding"/> from a XAML attribute value.
	/// </summary>
	[ContentProperty(nameof(Path))]
	[RequireService([typeof(IXamlTypeResolver), typeof(IXamlDataTypeProvider)])]
	public sealed class BindingExtension : IMarkupExtension<BindingBase>
	{
		/// <summary>
		/// Gets or sets the path to the binding source property.
		/// </summary>
		public string Path { get; set; } = Binding.SelfPath;

		/// <summary>
		/// Gets or sets the binding mode.
		/// </summary>
		public BindingMode Mode { get; set; } = BindingMode.Default;

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

		/// <summary>
		/// Gets or sets the source object for the binding.
		/// </summary>
		public object Source { get; set; }

		/// <summary>
		/// Gets or sets the name of the event that triggers the source update in TwoWay bindings.
		/// </summary>
		public string UpdateSourceEventName { get; set; }

		/// <summary>
		/// Gets or sets the value to use when the target property value is <see langword="null"/>.
		/// </summary>
		public object TargetNullValue { get; set; }

		/// <summary>
		/// Gets or sets the value to use when the binding cannot return a value.
		/// </summary>
		public object FallbackValue { get; set; }

		/// <summary>For internal use only. This API can be changed or removed without notice at any time.</summary>
		[EditorBrowsable(EditorBrowsableState.Never)] public TypedBindingBase TypedBinding { get; set; }

		BindingBase IMarkupExtension<BindingBase>.ProvideValue(IServiceProvider serviceProvider)
		{
			if (TypedBinding is null)
			{
				return CreateBinding();
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

			[UnconditionalSuppressMessage("TrimAnalysis", "IL2026",
				Justification = "If this method is invoked, we have already produced warnings in XamlC " +
					"when the compilation of this binding failed or was skipped.")]
			BindingBase CreateBinding()
			{
				Type bindingXDataType = null;
				if (serviceProvider is not null &&
					(serviceProvider.GetService(typeof(IXamlTypeResolver)) is IXamlTypeResolver typeResolver)
					&& (serviceProvider.GetService(typeof(IXamlDataTypeProvider)) is IXamlDataTypeProvider dataTypeProvider)
					&& dataTypeProvider.BindingDataType != null)
				{
					typeResolver.TryResolve(dataTypeProvider.BindingDataType, out bindingXDataType);
				}
				return new Binding(Path, Mode, Converter, ConverterParameter, StringFormat, Source)
				{
					UpdateSourceEventName = UpdateSourceEventName,
					FallbackValue = FallbackValue,
					TargetNullValue = TargetNullValue,
					DataType = bindingXDataType,
				};
			}
		}

		object IMarkupExtension.ProvideValue(IServiceProvider serviceProvider)
		{
			return (this as IMarkupExtension<BindingBase>).ProvideValue(serviceProvider);
		}
	}
}