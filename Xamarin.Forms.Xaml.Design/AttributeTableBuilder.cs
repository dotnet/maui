using System;
using System.ComponentModel;
using Microsoft.Windows.Design;

namespace Xamarin.Forms.Xaml.Design
{
	class AttributeTableBuilder : Microsoft.Windows.Design.Metadata.AttributeTableBuilder
	{
		public AttributeTableBuilder()
		{
			AddCustomAttributes(typeof(ArrayExtension).Assembly,
				new XmlnsSupportsValidationAttribute("http://xamarin.com/schemas/2014/forms", false));

			AddCallback(typeof(OnPlatformExtension), builder => builder.AddCustomAttributes(new Attribute[] {
				new System.Windows.Markup.MarkupExtensionReturnTypeAttribute (),
			}));
			AddCallback(typeof(OnIdiomExtension), builder => builder.AddCustomAttributes(new Attribute[] {
				new System.Windows.Markup.MarkupExtensionReturnTypeAttribute (),
			}));
		}
	}
}