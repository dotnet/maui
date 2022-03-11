using System;
using System.Globalization;
using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	internal class ContentConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			var presenter = parameter as ContentPresenter;

			if (value is View view)
			{
				return ConfigureView(view, presenter);
			}

			if (value is string textContent)
			{
				return ConvertToLabel(textContent, presenter);
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		static View ConfigureView(View view, ContentPresenter presenter)
		{
			if (view is ITextElement && HasTemplateAncestor(presenter, typeof(ITextElement)))
			{
				BindTextProperties(view);
			}

			if (view is IFontElement && HasTemplateAncestor(presenter, typeof(IFontElement)))
			{
				BindFontProperties(view);
			}

			return view;
		}

		static Label ConvertToLabel(string textContent, ContentPresenter presenter)
		{
			var label = new Label
			{
				Text = textContent
			};

			if (HasTemplateAncestor(presenter, typeof(ITextElement)))
			{
				BindTextProperties(label);
			}

			if (HasTemplateAncestor(presenter, typeof(IFontElement)))
			{
				BindFontProperties(label);
			}

			return label;
		}

		static void BindTextProperties(BindableObject content)
		{
			BindProperty(content, TextElement.TextColorProperty, typeof(ITextElement));
			BindProperty(content, TextElement.CharacterSpacingProperty, typeof(ITextElement));
			BindProperty(content, TextElement.TextTransformProperty, typeof(ITextElement));
		}

		static void BindFontProperties(BindableObject content)
		{
			BindProperty(content, FontElement.FontAttributesProperty, typeof(IFontElement));
			BindProperty(content, FontElement.FontSizeProperty, typeof(IFontElement));
			BindProperty(content, FontElement.FontFamilyProperty, typeof(IFontElement));
		}

		static void BindProperty(BindableObject content, BindableProperty property, Type type)
		{
			if (content.IsSet(property) || content.GetIsBound(property))
			{
				// Don't override the property if user has already set it
				return;
			}

			content.SetBinding(property,
					new Binding(property.PropertyName,
					source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, type)));
		}

		static bool HasTemplateAncestor(ContentPresenter presenter, Type type)
		{
			var parent = presenter?.Parent;

			while (parent != null)
			{
				if (type.IsAssignableFrom(parent.GetType()))
				{
					return true;
				}

				if (parent is ContentView)
				{
					break;
				}

				parent = parent.Parent;
			}

			return false;
		}
	}
}