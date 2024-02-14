#nullable disable
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
			BindProperty(content, TextElement.TextColorProperty, static (ITextElement te) => te.TextColor);
			BindProperty(content, TextElement.CharacterSpacingProperty, static (ITextElement te) => te.CharacterSpacing);
			BindProperty(content, TextElement.TextTransformProperty, static (ITextElement te) => te.TextTransform);
		}

		static void BindFontProperties(BindableObject content)
		{
			BindProperty(content, FontElement.FontAttributesProperty, static (IFontElement fe) => fe.FontAttributes);
			BindProperty(content, FontElement.FontSizeProperty, static (IFontElement fe) => fe.FontSize);
			BindProperty(content, FontElement.FontFamilyProperty, static (IFontElement fe) => fe.FontFamily);
		}

		static void BindProperty<TSource, TProperty>(
			BindableObject content,
			BindableProperty property,
			Func<TSource, TProperty> getter)
		{
			if (content.IsSet(property) || content.GetIsBound(property))
			{
				// Don't override the property if user has already set it
				return;
			}

			content.SetBinding(property, TypedBinding.ForSingleNestingLevel(
				property.PropertyName, getter, source: new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(TSource))));
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