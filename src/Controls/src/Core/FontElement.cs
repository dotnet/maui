using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
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

		public static readonly BindableProperty FontAutoScalingEnabledProperty =
			BindableProperty.Create("FontAutoScalingEnabled", typeof(bool), typeof(IFontElement), true,
									propertyChanged: OnFontAutoScalingEnabledChanged);

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
				bindable.ClearValue(FontAutoScalingEnabledProperty);
			}
			else
			{
				bindable.SetValue(FontFamilyProperty, font.Family);
				bindable.SetValue(FontSizeProperty, font.Size);
				bindable.SetValue(FontAttributesProperty, font.GetFontAttributes());
				bindable.SetValue(FontAutoScalingEnabledProperty, font.AutoScalingEnabled);
			}

			SetCancelEvents(bindable, false);
		}

		static bool OnChanged(BindableObject bindable)
		{
			if (GetCancelEvents(bindable))
				return false;

			IFontElement fontElement = (IFontElement)bindable;

			SetCancelEvents(bindable, true);
			bindable.SetValue(FontProperty, fontElement.ToFont());

			SetCancelEvents(bindable, false);
			return true;
		}

		static void OnFontFamilyChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (!OnChanged(bindable))
				return;

			((IFontElement)bindable).OnFontFamilyChanged((string)oldValue, (string)newValue);
		}

		static void OnFontSizeChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (!OnChanged(bindable))
				return;

			((IFontElement)bindable).OnFontSizeChanged((double)oldValue, (double)newValue);
		}

		static object FontSizeDefaultValueCreator(BindableObject bindable)
		{
			return ((IFontElement)bindable).FontSizeDefaultValueCreator();
		}

		static void OnFontAttributesChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (!OnChanged(bindable))
				return;

			((IFontElement)bindable).OnFontAttributesChanged((FontAttributes)oldValue, (FontAttributes)newValue);
		}

		static void OnFontAutoScalingEnabledChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (!OnChanged(bindable))
				return;

			((IFontElement)bindable).OnFontAutoScalingEnabledChanged((bool)oldValue, (bool)newValue);
		}
	}
}
