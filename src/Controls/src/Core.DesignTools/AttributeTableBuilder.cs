using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;
using Microsoft.VisualStudio.DesignTools.Extensibility;

namespace Microsoft.Maui.Controls.Core.Design
{
	internal class AttributeTableBuilder : Microsoft.VisualStudio.DesignTools.Extensibility.Metadata.AttributeTableBuilder
	{
		public AttributeTableBuilder()
		{
			// Turn off validation of values, which doesn't work for OnPlatform/OnIdiom
			AddCustomAttributes("Microsoft.Maui.Controls.AbsoluteLayout",
				new XmlnsSupportsValidationAttribute("http://schemas.microsoft.com/dotnet/2021/maui", false));

			// Style isn't a view, make it visible
			AddCallback("Microsoft.Maui.Controls.Style", builder => builder.AddCustomAttributes(
			   new EditorBrowsableAttribute(EditorBrowsableState.Always),
			   new ContentPropertyAttribute("Setters"),
			   // Since the class doesn't have a public parameterless ctor, we need to provide a converter
			   new TypeConverterAttribute(typeof(StringConverter))));

			// The Setter.Value can actually come from an <OnPlatform />, so enable it as Content.
			AddCallback("Microsoft.Maui.Controls.Setter", builder => builder.AddCustomAttributes(
			  new EditorBrowsableAttribute(EditorBrowsableState.Always),
			  new ContentPropertyAttribute("Value")));

			// Special case for FontSize which isn't an enum.
			var ifontElementType = Type.GetType("Microsoft.Maui.Controls.Internals.IFontElement,Microsoft.Maui.Controls"); // typeof(IVisual);
			var fontElements = ifontElementType.Assembly.ExportedTypes.Where(t => ifontElementType.IsAssignableFrom(t));
			foreach (var fontElement in fontElements)
			{
				AddCallback(fontElement.FullName, builder => builder.AddCustomAttributes(
				   "FontSize",
				   new TypeConverterAttribute(typeof(NamedFontSizeEnumConverter))));
			}

			AddCallback("Microsoft.Maui.Controls.VisualElement", builder => builder.AddCustomAttributes(
			   "Visual",
			   new TypeConverterAttribute(typeof(VisualDesignTypeConverter))));

			AddCallback("Microsoft.Maui.Controls.ItemsView", builder => builder.AddCustomAttributes(
				"ItemsLayout",
			   new TypeConverterAttribute(typeof(ItemsLayoutDesignTypeConverter))));

			//AddCallback("Microsoft.Maui.Controls.InputView", builder => builder.AddCustomAttributes(
			//   "Microsoft.Maui.Controls.Keyboard",
			//   new System.ComponentModel.TypeConverterAttribute(typeof(KeyboardDesignTypeConverter))));

			//AddCallback("Microsoft.Maui.Controls.EntryCell", builder => builder.AddCustomAttributes(
			//   "Keyboard",
			//   new System.ComponentModel.TypeConverterAttribute(typeof(KeyboardDesignTypeConverter))));

			// TODO: OnPlatform/OnIdiom
			// These two should be proper markup extensions, to follow WPF syntax for those.
			// That would allow us to turn on XAML validation, which otherwise fails.
			// NOTE: the two also need to provide a non-generic, object-based T so that 
			// the language service can find the type by its name. That class can be internal 
			// though, since its visibility in the markup is controlled by the EditorBrowsableAttribute.
			// Make OnPlatform/OnIdiom visible for intellisense, and set as markup extension. 
			AddCallback("Microsoft.Maui.Controls.OnPlatform<>", builder => builder.AddCustomAttributes(new Attribute[] {
				new EditorBrowsableAttribute (EditorBrowsableState.Always),
				//new System.ComponentModel.TypeConverterAttribute(typeof(AnythingConverter)),
				//new System.Windows.Markup.MarkupExtensionReturnTypeAttribute (),
			}));
			AddCallback("Microsoft.Maui.Controls.OnIdiom<>", builder => builder.AddCustomAttributes(new Attribute[] {
				new EditorBrowsableAttribute (EditorBrowsableState.Always), 
				//new System.ComponentModel.TypeConverterAttribute(typeof(AnythingConverter)),
				//new System.Windows.Markup.MarkupExtensionReturnTypeAttribute (),
			}));
		}
	}

	internal class AnythingConverter : System.ComponentModel.TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
			=> true;

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
			=> true;

		public override bool IsValid(ITypeDescriptorContext context, object value)
			=> true;
	}
}