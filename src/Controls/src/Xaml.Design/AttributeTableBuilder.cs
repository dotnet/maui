using System;
using System.ComponentModel;
using System.Windows.Markup;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Metadata;

namespace Microsoft.Maui.Controls.Xaml.Design
{
	class AttributeTableBuilder : Microsoft.Windows.Design.Metadata.AttributeTableBuilder
	{
		public AttributeTableBuilder()
		{
			AddCustomAttributes(typeof(ArrayExtension).Assembly,
				new XmlnsSupportsValidationAttribute("http://schemas.microsoft.com/dotnet/2021/maui", false));

			AddCallback(typeof(OnPlatformExtension), builder => builder.AddCustomAttributes(new Attribute[] {
				new System.Windows.Markup.MarkupExtensionReturnTypeAttribute (),
			}));
			AddCallback(typeof(OnIdiomExtension), builder => builder.AddCustomAttributes(new Attribute[] {
				new System.Windows.Markup.MarkupExtensionReturnTypeAttribute (),
			}));

			Type typeFromHandle = typeof(AppThemeBindingExtension);
			AddTypeAttributes(typeFromHandle, new MarkupExtensionReturnTypeAttribute());
			typeFromHandle = typeof(ArrayExtension);
			AddTypeAttributes(typeFromHandle, new MarkupExtensionReturnTypeAttribute());
			typeFromHandle = typeof(BindingExtension);
			AddTypeAttributes(typeFromHandle, new MarkupExtensionReturnTypeAttribute());
			typeFromHandle = typeof(DataTemplateExtension);
			AddTypeAttributes(typeFromHandle, new MarkupExtensionReturnTypeAttribute());
			typeFromHandle = typeof(DynamicResourceExtension);
			AddTypeAttributes(typeFromHandle, new MarkupExtensionReturnTypeAttribute());
			typeFromHandle = typeof(FontImageExtension);
			AddTypeAttributes(typeFromHandle, new MarkupExtensionReturnTypeAttribute());
			typeFromHandle = typeof(NullExtension);
			AddTypeAttributes(typeFromHandle, new MarkupExtensionReturnTypeAttribute());
			typeFromHandle = typeof(OnIdiomExtension);
			AddTypeAttributes(typeFromHandle, new MarkupExtensionReturnTypeAttribute());
			typeFromHandle = typeof(OnPlatformExtension);
			AddTypeAttributes(typeFromHandle, new MarkupExtensionReturnTypeAttribute());
			typeFromHandle = typeof(ReferenceExtension);
			AddTypeAttributes(typeFromHandle, new MarkupExtensionReturnTypeAttribute());
			typeFromHandle = typeof(RelativeSourceExtension);
			AddTypeAttributes(typeFromHandle, new MarkupExtensionReturnTypeAttribute());
			typeFromHandle = typeof(StaticExtension);
			AddTypeAttributes(typeFromHandle, new MarkupExtensionReturnTypeAttribute());
			typeFromHandle = typeof(StaticResourceExtension);
			AddTypeAttributes(typeFromHandle, new MarkupExtensionReturnTypeAttribute());
			typeFromHandle = typeof(TemplateBindingExtension);
			AddTypeAttributes(typeFromHandle, new MarkupExtensionReturnTypeAttribute());
			typeFromHandle = typeof(TypeExtension);
			AddTypeAttributes(typeFromHandle, new MarkupExtensionReturnTypeAttribute());
		}

		private void AddTypeAttributes(Type type, params Attribute[] attribs)
		{
			AddCallback(type, delegate (AttributeCallbackBuilder builder)
			{
				builder.AddCustomAttributes(attribs);
			});
		}
	}
}