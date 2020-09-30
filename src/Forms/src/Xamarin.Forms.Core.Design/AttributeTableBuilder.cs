using System;
using System.ComponentModel;
using System.Linq;
using System.Windows.Markup;
using Microsoft.Windows.Design;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms.Core.Design
{
	internal class AttributeTableBuilder : Microsoft.Windows.Design.Metadata.AttributeTableBuilder
	{
		public AttributeTableBuilder()
		{
			// Turn off validation of values, which doesn't work for OnPlatform/OnIdiom
			AddCustomAttributes(typeof(AbsoluteLayout).Assembly,
				new XmlnsSupportsValidationAttribute("http://xamarin.com/schemas/2014/forms", false));

			// Style isn't a view, make it visible
			AddCallback(typeof(Style), builder => builder.AddCustomAttributes(
			   new EditorBrowsableAttribute(EditorBrowsableState.Always),
			   new System.Windows.Markup.ContentPropertyAttribute("Setters"),
			   // Since the class doesn't have a public parameterless ctor, we need to provide a converter
			   new System.ComponentModel.TypeConverterAttribute(typeof(StringConverter))));

			// The Setter.Value can actually come from an <OnPlatform />, so enable it as Content.
			AddCallback(typeof(Setter), builder => builder.AddCustomAttributes(
			  new EditorBrowsableAttribute(EditorBrowsableState.Always),
			  new System.Windows.Markup.ContentPropertyAttribute("Value")));

			// Special case for FontSize which isn't an enum.
			var fontElements = typeof(View).Assembly.ExportedTypes.Where(t => typeof(IFontElement).IsAssignableFrom(t));
			foreach (var fontElement in fontElements)
			{
				AddCallback(fontElement, builder => builder.AddCustomAttributes(
				   "FontSize",
				   new System.ComponentModel.TypeConverterAttribute(typeof(NonExclusiveEnumConverter<NamedSize>))));
			}

			AddCallback(typeof(VisualElement), builder => builder.AddCustomAttributes(
			   "Visual",
			   new System.ComponentModel.TypeConverterAttribute(typeof(VisualDesignTypeConverter))));

			AddCallback(typeof(ItemsView), builder => builder.AddCustomAttributes(
				"ItemsLayout",
			   new System.ComponentModel.TypeConverterAttribute(typeof(ItemsLayoutDesignTypeConverter))));

			AddCallback(typeof(InputView), builder => builder.AddCustomAttributes(
			   nameof(Keyboard),
			   new System.ComponentModel.TypeConverterAttribute(typeof(KeyboardDesignTypeConverter))));

			AddCallback(typeof(EntryCell), builder => builder.AddCustomAttributes(
			   nameof(Keyboard),
			   new System.ComponentModel.TypeConverterAttribute(typeof(KeyboardDesignTypeConverter))));

			// TODO: OnPlatform/OnIdiom
			// These two should be proper markup extensions, to follow WPF syntax for those.
			// That would allow us to turn on XAML validation, which otherwise fails.
			// NOTE: the two also need to provide a non-generic, object-based T so that 
			// the language service can find the type by its name. That class can be internal 
			// though, since its visibility in the markup is controlled by the EditorBrowsableAttribute.
			// Make OnPlatform/OnIdiom visible for intellisense, and set as markup extension. 
			AddCallback(typeof(OnPlatform<>), builder => builder.AddCustomAttributes(new Attribute[] {
				new EditorBrowsableAttribute (EditorBrowsableState.Always),
				//new System.ComponentModel.TypeConverterAttribute(typeof(AnythingConverter)),
				//new System.Windows.Markup.MarkupExtensionReturnTypeAttribute (),
			}));
			AddCallback(typeof(OnIdiom<>), builder => builder.AddCustomAttributes(new Attribute[] {
				new EditorBrowsableAttribute (EditorBrowsableState.Always), 
				//new System.ComponentModel.TypeConverterAttribute(typeof(AnythingConverter)),
				//new System.Windows.Markup.MarkupExtensionReturnTypeAttribute (),
			}));
		}
	}

	internal class AnythingConverter : System.ComponentModel.TypeConverter
	{
		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return true;
		}

		public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
		{
			return true;
		}

		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			return true;
		}
	}
}