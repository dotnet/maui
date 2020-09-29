using Xamarin.Forms.Internals;

namespace Xamarin.Forms
{
	static class FontElement
	{
		public static readonly BindableProperty FontProperty =
			BindableProperty.Create("Font", typeof(Font), typeof(IFontElement), default(Font),
									propertyChanged: OnFontPropertyChanged);

		public static readonly BindableProperty FontFamilyProperty =
			BindableProperty.Create("FontFamily", typeof(string), typeof(IFontElement), default(string),
									propertyChanged: OnFontFamilyChanged);

		public static readonly BindableProperty FontSizeProperty =
			BindableProperty.Create("FontSize", typeof(double), typeof(IFontElement), -1.0,
									propertyChanged: OnFontSizeChanged,
									defaultValueCreator: FontSizeDefaultValueCreator);

		public static readonly BindableProperty FontAttributesProperty =
			BindableProperty.Create("FontAttributes", typeof(FontAttributes), typeof(IFontElement), FontAttributes.None,
									propertyChanged: OnFontAttributesChanged);

		static readonly BindableProperty CancelEventsProperty =
			BindableProperty.Create("CancelEvents", typeof(bool), typeof(FontElement), false);

		static bool GetCancelEvents(BindableObject bindable) => (bool)bindable.GetValue(CancelEventsProperty);
		static void SetCancelEvents(BindableObject bindable, bool value)
		{
			bindable.SetValue(CancelEventsProperty, value);
		}

		static void OnFontPropertyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (GetCancelEvents(bindable))
				return;

			SetCancelEvents(bindable, true);

			var font = (Font)newValue;
			if (font == Font.Default)
			{
				bindable.ClearValue(FontFamilyProperty);
				bindable.ClearValue(FontSizeProperty);
				bindable.ClearValue(FontAttributesProperty);
			}
			else
			{
				bindable.SetValue(FontFamilyProperty, font.FontFamily);
				if (font.UseNamedSize)
					bindable.SetValue(FontSizeProperty, Device.GetNamedSize(font.NamedSize, bindable.GetType(), true));
				else
					bindable.SetValue(FontSizeProperty, font.FontSize);
				bindable.SetValue(FontAttributesProperty, font.FontAttributes);
			}
			SetCancelEvents(bindable, false);
		}

		static void OnFontFamilyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (GetCancelEvents(bindable))
				return;

			SetCancelEvents(bindable, true);

			var fontSize = (double)bindable.GetValue(FontSizeProperty);
			var fontAttributes = (FontAttributes)bindable.GetValue(FontAttributesProperty);
			var fontFamily = (string)newValue;

			if (fontFamily != null)
				bindable.SetValue(FontProperty, Font.OfSize(fontFamily, fontSize).WithAttributes(fontAttributes));
			else
				bindable.SetValue(FontProperty, Font.SystemFontOfSize(fontSize, fontAttributes));

			SetCancelEvents(bindable, false);
			((IFontElement)bindable).OnFontFamilyChanged((string)oldValue, (string)newValue);
		}

		static void OnFontSizeChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (GetCancelEvents(bindable))
				return;

			SetCancelEvents(bindable, true);

			var fontSize = (double)newValue;
			var fontAttributes = (FontAttributes)bindable.GetValue(FontAttributesProperty);
			var fontFamily = (string)bindable.GetValue(FontFamilyProperty);

			if (fontFamily != null)
				bindable.SetValue(FontProperty, Font.OfSize(fontFamily, fontSize).WithAttributes(fontAttributes));
			else
				bindable.SetValue(FontProperty, Font.SystemFontOfSize(fontSize, fontAttributes));

			SetCancelEvents(bindable, false);
			((IFontElement)bindable).OnFontSizeChanged((double)oldValue, (double)newValue);
		}

		static object FontSizeDefaultValueCreator(BindableObject bindable)
		{
			return ((IFontElement)bindable).FontSizeDefaultValueCreator();
		}

		static void OnFontAttributesChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (GetCancelEvents(bindable))
				return;

			SetCancelEvents(bindable, true);

			var fontSize = (double)bindable.GetValue(FontSizeProperty);
			var fontAttributes = (FontAttributes)newValue;
			var fontFamily = (string)bindable.GetValue(FontFamilyProperty);

			if (fontFamily != null)
				bindable.SetValue(FontProperty, Font.OfSize(fontFamily, fontSize).WithAttributes(fontAttributes));
			else
				bindable.SetValue(FontProperty, Font.SystemFontOfSize(fontSize, fontAttributes));

			SetCancelEvents(bindable, false);
			((IFontElement)bindable).OnFontAttributesChanged((FontAttributes)oldValue, (FontAttributes)newValue);
		}
	}
}