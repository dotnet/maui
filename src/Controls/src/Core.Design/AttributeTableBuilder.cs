using System;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Windows.Markup;
using Microsoft.Maui.Controls.Internals;
using Microsoft.Maui.Controls.Layout2;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;
using Microsoft.Windows.Design;
using Microsoft.Windows.Design.Metadata;
using Rectangle = Microsoft.Maui.Graphics.Rectangle;

namespace Microsoft.Maui.Controls.Core.Design
{
	internal class AttributeTableBuilder : Microsoft.Windows.Design.Metadata.AttributeTableBuilder
	{
		public AttributeTableBuilder()
		{
			// Turn off validation of values, which doesn't work for OnPlatform/OnIdiom
			AddCustomAttributes(typeof(AbsoluteLayout).Assembly,
				new XmlnsSupportsValidationAttribute("http://schemas.microsoft.com/dotnet/2021/maui", false));

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

		private void AddAttributesForTypes()
		{
			Type typeFromHandle = typeof(AbsoluteLayout);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Children"));
			typeFromHandle = typeof(ActivityIndicator);
			typeFromHandle = typeof(BoxView);
			typeFromHandle = typeof(Button);
			typeFromHandle = typeof(CarouselPage);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Children"));
			typeFromHandle = typeof(CarouselView);
			typeFromHandle = typeof(CheckBox);
			typeFromHandle = typeof(CollectionView);
			typeFromHandle = typeof(Color);
			AddTypeAttributes(typeFromHandle, new TypeConverterAttribute(typeof(NamedColorsConverter)));
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Content"));
			typeFromHandle = typeof(ConstraintExpression);
			AddTypeAttributes(typeFromHandle, new MarkupExtensionReturnTypeAttribute());
			typeFromHandle = typeof(ContentPage);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Content"));
			typeFromHandle = typeof(ContentPresenter);
			typeFromHandle = typeof(ContentView);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Content"));
			typeFromHandle = typeof(DatePicker);
			typeFromHandle = typeof(Easing);
			AddTypeAttributes(typeFromHandle, new TypeConverterAttribute(typeof(StructOptionsConverter<Easing>)));
			typeFromHandle = typeof(Editor);
			typeFromHandle = typeof(Ellipse);
			typeFromHandle = typeof(Entry);
			typeFromHandle = typeof(FlexLayout);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Children"));
			typeFromHandle = typeof(FlyoutPage);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Detail"));
			typeFromHandle = typeof(Frame);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Content"));
			typeFromHandle = typeof(Grid);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Children"));

			typeFromHandle = typeof(GridLayout);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Children"));
			typeFromHandle = typeof(Layout2.Layout);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Children"));
			typeFromHandle = typeof(HorizontalStackLayout);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Children"));
			typeFromHandle = typeof(VerticalStackLayout);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Children"));

			typeFromHandle = typeof(GroupableItemsView);
			typeFromHandle = typeof(Image);
			typeFromHandle = typeof(ImageButton);
			typeFromHandle = typeof(IndicatorView);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("IndicatorLayout"));
			typeFromHandle = typeof(InputView);
			AddTypeAttributes(typeFromHandle, new TypeConverterAttribute(typeof(StringConverter)));
			typeFromHandle = typeof(Label);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Text"));
			typeFromHandle = typeof(LayoutOptions);
			AddTypeAttributes(typeFromHandle, new TypeConverterAttribute(typeof(StructOptionsConverter<LayoutOptions>)));
			typeFromHandle = typeof(Line);
			typeFromHandle = typeof(LinearItemsLayout);
			AddTypeAttributes(typeFromHandle, new TypeConverterAttribute(typeof(StructOptionsConverter<LinearItemsLayout>)));
			typeFromHandle = typeof(ListView);
			typeFromHandle = typeof(FlyoutPage);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Detail"));
#pragma warning disable CS0618 // Type or member is obsolete
			typeFromHandle = typeof(MasterDetailPage);
#pragma warning restore CS0618 // Type or member is obsolete
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Detail"));
			typeFromHandle = typeof(NavigationPage);
			typeFromHandle = typeof(OpenGLView);
			typeFromHandle = typeof(Page);
			typeFromHandle = typeof(Path);
			typeFromHandle = typeof(Picker);
			typeFromHandle = typeof(Polygon);
			typeFromHandle = typeof(Polyline);
			typeFromHandle = typeof(ProgressBar);
			typeFromHandle = typeof(RadioButton);
			typeFromHandle = typeof(Rectangle);
			typeFromHandle = typeof(RefreshView);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Content"));
			typeFromHandle = typeof(RelativeLayout);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Children"));
			typeFromHandle = typeof(ResourcesChangedEventArgs);
			AddTypeAttributes(typeFromHandle, new TypeConverterAttribute(typeof(StructOptionsConverter<ResourcesChangedEventArgs>)));
			typeFromHandle = typeof(ScrollView);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Content"));
			typeFromHandle = typeof(SearchBar);
			typeFromHandle = typeof(SelectableItemsView);
			typeFromHandle = typeof(Shell);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Items"));
			typeFromHandle = typeof(Size);
			AddTypeAttributes(typeFromHandle, new TypeConverterAttribute(typeof(StructOptionsConverter<Size>)));
			typeFromHandle = typeof(Slider);
			typeFromHandle = typeof(StackLayout);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Children"));
			typeFromHandle = typeof(Stepper);
			typeFromHandle = typeof(StructuredItemsView);
			typeFromHandle = typeof(SwipeItemView);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Content"));
			typeFromHandle = typeof(SwipeView);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Content"));
			typeFromHandle = typeof(Switch);
			typeFromHandle = typeof(TabbedPage);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Children"));
			typeFromHandle = typeof(TableView);
			AddTypeAttributes(typeFromHandle, new ContentPropertyAttribute("Root"));
			typeFromHandle = typeof(TemplatedPage);
			typeFromHandle = typeof(TemplatedView);
			typeFromHandle = typeof(TimePicker);
			typeFromHandle = typeof(View);
			AddTypeAttributes(typeFromHandle, new TypeConverterAttribute(typeof(StringConverter)));
			typeFromHandle = typeof(WebView);

			AddCustomAttributes(typeof(AbsoluteLayout).Assembly, new XmlnsSupportsValidationAttribute("http://schemas.microsoft.com/dotnet/2021/maui", supportsValidation: false));
			AddCallback(typeof(Style), delegate (AttributeCallbackBuilder builder)
			{
				builder.AddCustomAttributes(new EditorBrowsableAttribute(EditorBrowsableState.Always), new ContentPropertyAttribute("Setters"), new TypeConverterAttribute(typeof(StringConverter)));
			});
			AddCallback(typeof(Setter), delegate (AttributeCallbackBuilder builder)
			{
				builder.AddCustomAttributes(new EditorBrowsableAttribute(EditorBrowsableState.Always), new ContentPropertyAttribute("Value"));
			});
			foreach (Type item in from t in typeof(View).Assembly.ExportedTypes
								  where typeof(IFontElement).IsAssignableFrom(t)
								  select t)
			{
				AddCallback(item, delegate (AttributeCallbackBuilder builder)
				{
					builder.AddCustomAttributes("FontSize", new TypeConverterAttribute(typeof(NonExclusiveEnumConverter<NamedSize>)));
				});
			}
			AddCallback(typeof(VisualElement), delegate (AttributeCallbackBuilder builder)
			{
				builder.AddCustomAttributes("Visual", new TypeConverterAttribute(typeof(VisualDesignTypeConverter)));
			});
			AddCallback(typeof(ItemsView), delegate (AttributeCallbackBuilder builder)
			{
				builder.AddCustomAttributes("ItemsLayout", new TypeConverterAttribute(typeof(ItemsLayoutDesignTypeConverter)));
			});
			AddCallback(typeof(InputView), delegate (AttributeCallbackBuilder builder)
			{
				builder.AddCustomAttributes("Keyboard", new TypeConverterAttribute(typeof(KeyboardDesignTypeConverter)));
			});
			AddCallback(typeof(EntryCell), delegate (AttributeCallbackBuilder builder)
			{
				builder.AddCustomAttributes("Keyboard", new TypeConverterAttribute(typeof(KeyboardDesignTypeConverter)));
			});
			AddCallback(typeof(OnPlatform<>), delegate (AttributeCallbackBuilder builder)
			{
				builder.AddCustomAttributes(new EditorBrowsableAttribute(EditorBrowsableState.Always));
			});
			AddCallback(typeof(OnIdiom<>), delegate (AttributeCallbackBuilder builder)
			{
				builder.AddCustomAttributes(new EditorBrowsableAttribute(EditorBrowsableState.Always));
			});
		}

		private void AddTypeAttributes(Type type, params Attribute[] attribs)
		{
			AddCallback(type, delegate (AttributeCallbackBuilder builder)
			{
				builder.AddCustomAttributes(attribs);
			});
		}
	}

	internal class StructOptionsConverter<T> : System.ComponentModel.TypeConverter
	{
		private static Lazy<string[]> StandardValues = new Lazy<string[]>(() => (from fi in typeof(T).GetFields().Where(delegate (FieldInfo fi)
		{
			if (fi.IsStatic && fi.IsPublic)
			{
				return !fi.CustomAttributes.Any((CustomAttributeData a) => a.AttributeType == typeof(ObsoleteAttribute));
			}
			return false;
		}) select fi.Name).Union(from pi in typeof(T).GetProperties().Where(delegate (PropertyInfo pi)
			{
				if (pi.GetAccessors(nonPublic: true)[0].IsStatic && pi.GetAccessors(nonPublic: true)[0].IsPublic)
				{
					return !pi.CustomAttributes.Any((CustomAttributeData a) => a.AttributeType == typeof(ObsoleteAttribute));
				}
				return false;
			}) select pi.Name).ToArray());

		public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
		{
			return sourceType == typeof(string);
		}

		public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
		{
			if (value != null && StandardValues.Value.Contains(value.ToString()))
			{
				return null;
			}
			return base.ConvertFrom(context, culture, value);
		}

		public override bool IsValid(ITypeDescriptorContext context, object value)
		{
			if (value == null)
			{
				return false;
			}
			if (!StandardValues.Value.Contains(value.ToString()))
			{
				return value.GetType().FullName.StartsWith("Microsoft.Maui.Controls.OnPlatform");
			}
			return true;
		}

		public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
		{
			return true;
		}

		public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
		{
			return false;
		}

		public override System.ComponentModel.TypeConverter.StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
		{
			return new System.ComponentModel.TypeConverter.StandardValuesCollection(StandardValues.Value);
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