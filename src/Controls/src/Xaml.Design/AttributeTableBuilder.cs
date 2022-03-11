using System;
using System.ComponentModel;
using System.Windows.Markup;
using Microsoft.VisualStudio.DesignTools.Extensibility;
using Microsoft.VisualStudio.DesignTools.Extensibility.Metadata;

namespace Microsoft.Maui.Controls.Xaml.Design
{
	class AttributeTableBuilder : Microsoft.VisualStudio.DesignTools.Extensibility.Metadata.AttributeTableBuilder
	{
		public AttributeTableBuilder()
		{
			AddAssemblyCustomAttributes("Microsoft.Maui.Controls.Xaml",
				new XmlnsSupportsValidationAttribute("http://schemas.microsoft.com/dotnet/2021/maui", false));

			AddTypeAttributes("Microsoft.Maui.Controls.Xaml.AppThemeBindingExtension", new MarkupExtensionReturnTypeAttribute());
			AddTypeAttributes("Microsoft.Maui.Controls.Xaml.ArrayExtension", new MarkupExtensionReturnTypeAttribute());
			AddTypeAttributes("Microsoft.Maui.Controls.Xaml.BindingExtension", new MarkupExtensionReturnTypeAttribute());
			AddTypeAttributes("Microsoft.Maui.Controls.Xaml.DataTemplateExtension", new MarkupExtensionReturnTypeAttribute());
			AddTypeAttributes("Microsoft.Maui.Controls.Xaml.DynamicResourceExtension", new MarkupExtensionReturnTypeAttribute());
			AddTypeAttributes("Microsoft.Maui.Controls.Xaml.FontImageExtension", new MarkupExtensionReturnTypeAttribute());
			AddTypeAttributes("Microsoft.Maui.Controls.Xaml.NullExtension", new MarkupExtensionReturnTypeAttribute());
			AddTypeAttributes("Microsoft.Maui.Controls.Xaml.OnIdiomExtension", new MarkupExtensionReturnTypeAttribute());
			AddTypeAttributes("Microsoft.Maui.Controls.Xaml.OnPlatformExtension", new MarkupExtensionReturnTypeAttribute());
			AddTypeAttributes("Microsoft.Maui.Controls.Xaml.ReferenceExtension", new MarkupExtensionReturnTypeAttribute());
			AddTypeAttributes("Microsoft.Maui.Controls.Xaml.RelativeSourceExtension", new MarkupExtensionReturnTypeAttribute());
			AddTypeAttributes("Microsoft.Maui.Controls.Xaml.StaticExtension", new MarkupExtensionReturnTypeAttribute());
			AddTypeAttributes("Microsoft.Maui.Controls.Xaml.StaticResourceExtension", new MarkupExtensionReturnTypeAttribute());
			AddTypeAttributes("Microsoft.Maui.Controls.Xaml.TemplateBindingExtension", new MarkupExtensionReturnTypeAttribute());
			AddTypeAttributes("Microsoft.Maui.Controls.Xaml.TypeExtension", new MarkupExtensionReturnTypeAttribute());
		}

		private void AddTypeAttributes(string typeName, params Attribute[] attribs)
		{
			AddCallback(typeName, delegate (AttributeCallbackBuilder builder)
			{
				builder.AddCustomAttributes(attribs);
			});
		}
	}
}