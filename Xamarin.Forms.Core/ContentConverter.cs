using System;
using System.Globalization;
using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	internal class ContentConverter : IValueConverter
	{
		public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
		{
			if (value is View view)
			{
				return ConfigureView(view);
			}

			if (value is string textContent)
			{
				return ConvertToLabel(textContent);
			}

			return value;
		}

		public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
		{
			throw new NotImplementedException();
		}

		View ConfigureView(View view) 
		{
			if (view is ITextElement)
			{
				BindTextProperties(view);
			}

			if (view is IFontElement)
			{
				BindFontProperties(view);
			}

			return view;
		}

		Label ConvertToLabel(string textContent) 
		{
			var label = new Label
			{
				Text = textContent
			};

			BindTextProperties(label);
			BindFontProperties(label);

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
	}
}