using System;
using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls.Xaml
{
	[ContentProperty(nameof(Path))]
	[RequireService([typeof(IXamlTypeResolver), typeof(IXamlDataTypeProvider)])]
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
				Justification = "This code is only reachable in XamlC compiled code when there is a missing x:DataType and the binding could not be compiled. " +
					"In that case, we produce a warning that the binding could not be compiled.")]
			BindingBase CreateBinding()
			{
				Type bindingXDataType = null;
				if ((serviceProvider.GetService(typeof(IXamlTypeResolver)) is IXamlTypeResolver typeResolver)
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