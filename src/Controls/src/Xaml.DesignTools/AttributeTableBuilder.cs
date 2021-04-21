using System;
using System.ComponentModel;
using Microsoft.VisualStudio.DesignTools.Extensibility;

namespace Microsoft.Maui.Controls.Xaml.Design
{
	class AttributeTableBuilder : Microsoft.VisualStudio.DesignTools.Extensibility.Metadata.AttributeTableBuilder
	{
		public AttributeTableBuilder()
		{
			AddCustomAttributes("Microsoft.Maui.Controls.Xaml.ArrayExtension",
				new XmlnsSupportsValidationAttribute("http://schemas.microsoft.com/dotnet/2021/maui", false));

			AddCallback("Microsoft.Maui.Controls.Xaml.OnPlatformExtension", builder => builder.AddCustomAttributes(new Attribute[] {
				new System.Windows.Markup.MarkupExtensionReturnTypeAttribute (),
			}));
			AddCallback("Microsoft.Maui.Controls.Xaml.OnIdiomExtension", builder => builder.AddCustomAttributes(new Attribute[] {
				new System.Windows.Markup.MarkupExtensionReturnTypeAttribute (),
			}));
		}
	}
}