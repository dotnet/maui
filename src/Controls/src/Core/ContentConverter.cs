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
			var source = new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(ITextElement));
			if (ShouldSetBinding(content, TextElement.TextColorProperty))
			{
				content.SetBinding(TextElement.TextColorProperty, static (ITextElement te) => te.TextColor, source: source);
			}

			if (ShouldSetBinding(content, TextElement.CharacterSpacingProperty))
			{
				content.SetBinding(TextElement.CharacterSpacingProperty, static (ITextElement te) => te.CharacterSpacing, source: source);
			}

			if (ShouldSetBinding(content, TextElement.TextTransformProperty))
			{
				content.SetBinding(TextElement.TextTransformProperty, static (ITextElement te) => te.TextTransform, source: source);
			}
		}

		static void BindFontProperties(BindableObject content)
		{
			var source = new RelativeBindingSource(RelativeBindingSourceMode.FindAncestor, typeof(IFontElement));
			if (ShouldSetBinding(content, FontElement.FontAttributesProperty))
			{
				content.SetBinding(FontElement.FontAttributesProperty, static (IFontElement fe) => fe.FontAttributes, source: source);
			}

			if (ShouldSetBinding(content, FontElement.FontSizeProperty))
			{
				content.SetBinding(FontElement.FontSizeProperty, static (IFontElement fe) => fe.FontSize, source: source);
			}

			if (ShouldSetBinding(content, FontElement.FontFamilyProperty))
			{
				content.SetBinding(FontElement.FontFamilyProperty, static (IFontElement fe) => fe.FontFamily, source: source);
			}
		}

		static bool ShouldSetBinding(BindableObject content, BindableProperty property)
		{
			return !content.IsSet(property) && !content.GetIsBound(property);
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