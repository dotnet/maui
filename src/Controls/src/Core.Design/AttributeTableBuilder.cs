using System;
using System.ComponentModel;
using System.Windows.Markup;
using Microsoft.VisualStudio.DesignTools.Extensibility;

namespace Microsoft.Maui.Controls.Design
{
	internal class AttributeTableBuilder : Microsoft.VisualStudio.DesignTools.Extensibility.Metadata.AttributeTableBuilder
	{
		public AttributeTableBuilder()
		{
			AddAssemblyCustomAttributes("Microsoft.Maui.Controls",
				new XmlnsSupportsValidationAttribute("http://schemas.microsoft.com/dotnet/2021/maui", false));

			AddAssemblyCustomAttributes("Microsoft.Maui.Graphics",
				new XmlnsSupportsValidationAttribute("http://schemas.microsoft.com/dotnet/2021/maui", false));

			// Style isn't a view, make it visible
			AddTypeAttributes("Microsoft.Maui.Controls.Style",
			   new EditorBrowsableAttribute(EditorBrowsableState.Always),
			   new ContentPropertyAttribute("Setters"),
			   // Since the class doesn't have a public parameterless ctor, we need to provide a converter
			   new TypeConverterAttribute(typeof(StringConverter)));

			// The Setter.Value can actually come from an <OnPlatform />, so enable it as Content.
			AddTypeAttributes("Microsoft.Maui.Controls.Setter",
			  new EditorBrowsableAttribute(EditorBrowsableState.Always),
			  new ContentPropertyAttribute("Value"));

			AddMemberAttributes("Microsoft.Maui.Controls.VisualElement", "Visual",
			   new TypeConverterAttribute(typeof(VisualDesignTypeConverter)));

			AddMemberAttributes("Microsoft.Maui.Controls.ItemsView", "ItemsLayout",
			   new TypeConverterAttribute(typeof(ItemsLayoutDesignTypeConverter)));

			AddMemberAttributes("Microsoft.Maui.Controls.InputView", "Keyboard",
			   new TypeConverterAttribute(typeof(KeyboardDesignTypeConverter)),
			   new TypeConverterAttribute(typeof(StringConverter)));

			AddMemberAttributes("Microsoft.Maui.Controls.EntryCell", "Keyboard",
			   new TypeConverterAttribute(typeof(KeyboardDesignTypeConverter)));

			// TODO: OnPlatform/OnIdiom
			// These two should be proper markup extensions, to follow WPF syntax for those.
			// That would allow us to turn on XAML validation, which otherwise fails.
			// NOTE: the two also need to provide a non-generic, object-based T so that 
			// the language service can find the type by its name. That class can be internal 
			// though, since its visibility in the markup is controlled by the EditorBrowsableAttribute.
			// Make OnPlatform/OnIdiom visible for intellisense, and set as markup extension. 
			AddTypeAttributes("Microsoft.Maui.Controls.OnPlatform<>",
				new EditorBrowsableAttribute (EditorBrowsableState.Always)
				//new System.ComponentModel.TypeConverterAttribute(typeof(AnythingConverter)),
				//new System.Windows.Markup.MarkupExtensionReturnTypeAttribute (),
			);
			AddTypeAttributes("Microsoft.Maui.Controls.OnIdiom<>",
				new EditorBrowsableAttribute(EditorBrowsableState.Always)
				//new System.ComponentModel.TypeConverterAttribute(typeof(AnythingConverter)),
				//new System.Windows.Markup.MarkupExtensionReturnTypeAttribute (),
			);

			AddTypeAttributes("Microsoft.Maui.Graphics.Color", new TypeConverterAttribute(typeof(ColorDesignTypeConverter)));

			AddTypeAttributes("Microsoft.Maui.Controls.ConstraintExpression", new MarkupExtensionReturnTypeAttribute());
			AddTypeAttributes("Microsoft.Maui.Controls.LayoutOptions", new TypeConverterAttribute(typeof(LayoutOptionsDesignTypeConverter)));
			AddTypeAttributes("Microsoft.Maui.Controls.LinearItemsLayout", new TypeConverterAttribute(typeof(LinearItemsLayoutDesignTypeConverter)));
			AddTypeAttributes("Microsoft.Maui.Controls.ResourcesChangedEventArgs", new TypeConverterAttribute(typeof(FontSizeDesignTypeConverter)));

			AddMemberAttributes("Microsoft.Maui.Controls.Button", "FontSize", new TypeConverterAttribute(typeof(FontSizeDesignTypeConverter)));
			AddMemberAttributes("Microsoft.Maui.Controls.DatePicker", "FontSize", new TypeConverterAttribute(typeof(FontSizeDesignTypeConverter)));
			AddMemberAttributes("Microsoft.Maui.Controls.Editor", "FontSize", new TypeConverterAttribute(typeof(FontSizeDesignTypeConverter)));
			AddMemberAttributes("Microsoft.Maui.Controls.Entry", "FontSize", new TypeConverterAttribute(typeof(FontSizeDesignTypeConverter)));
			AddMemberAttributes("Microsoft.Maui.Controls.Label", "FontSize", new TypeConverterAttribute(typeof(FontSizeDesignTypeConverter)));
			AddMemberAttributes("Microsoft.Maui.Controls.Picker", "FontSize", new TypeConverterAttribute(typeof(FontSizeDesignTypeConverter)));
			AddMemberAttributes("Microsoft.Maui.Controls.SearchBar", "FontSize", new TypeConverterAttribute(typeof(FontSizeDesignTypeConverter)));
			AddMemberAttributes("Microsoft.Maui.Controls.TimePicker", "FontSize", new TypeConverterAttribute(typeof(FontSizeDesignTypeConverter)));
			AddMemberAttributes("Microsoft.Maui.Controls.RadioButton", "FontSize", new TypeConverterAttribute(typeof(FontSizeDesignTypeConverter)));
			AddMemberAttributes("Microsoft.Maui.Controls.SearchHandler", "FontSize", new TypeConverterAttribute(typeof(FontSizeDesignTypeConverter)));
			AddMemberAttributes("Microsoft.Maui.Controls.Span", "FontSize", new TypeConverterAttribute(typeof(FontSizeDesignTypeConverter)));

		}

		private void AddTypeAttributes(string typeName, params Attribute[] attribs)
			=> AddCallback(typeName, builder => builder.AddCustomAttributes(attribs));

		private void AddMemberAttributes(string typeName, string memberName, params Attribute[] attribs)
			=> AddCallback(typeName, builder => builder.AddCustomAttributes(memberName, attribs));
	}
}