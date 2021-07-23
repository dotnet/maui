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

		public static readonly BindableProperty FontScalingEnableProperty =
			BindableProperty.Create("EnableFontScaling", typeof(bool), typeof(IFontElement), true,
									propertyChanged: OnEnableFontScalingChanged);

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
				bindable.ClearValue(FontScalingEnableProperty);
			}
			else
			{
				bindable.SetValue(FontFamilyProperty, font.Family);
				bindable.SetValue(FontSizeProperty, font.Size);
				bindable.SetValue(FontAttributesProperty, font.GetFontAttributes());
				bindable.SetValue(FontScalingEnableProperty, font.EnableScaling);
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
			var enableAutoScaling = (bool)bindable.GetValue(FontScalingEnableProperty);

			if (fontFamily != null)
				bindable.SetValue(FontProperty, Font.OfSize(fontFamily, fontSize, enableScaling: enableAutoScaling).WithAttributes(fontAttributes));
			else
				bindable.SetValue(FontProperty, Font.SystemFontOfSize(fontSize, enableScaling: enableAutoScaling).WithAttributes(fontAttributes));

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
			var enableAutoScaling = (bool)bindable.GetValue(FontScalingEnableProperty);

			if (fontFamily != null)
				bindable.SetValue(FontProperty, Font.OfSize(fontFamily, fontSize, enableScaling: enableAutoScaling).WithAttributes(fontAttributes));
			else
				bindable.SetValue(FontProperty, Font.SystemFontOfSize(fontSize, enableScaling: enableAutoScaling).WithAttributes(fontAttributes));

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
			var enableAutoScaling = (bool)bindable.GetValue(FontScalingEnableProperty);

			if (fontFamily != null)
				bindable.SetValue(FontProperty, Font.OfSize(fontFamily, fontSize, enableScaling: enableAutoScaling).WithAttributes(fontAttributes));
			else
				bindable.SetValue(FontProperty, Font.SystemFontOfSize(fontSize, enableScaling: enableAutoScaling).WithAttributes(fontAttributes));

			SetCancelEvents(bindable, false);
			((IFontElement)bindable).OnFontAttributesChanged((FontAttributes)oldValue, (FontAttributes)newValue);
		}

		static void OnEnableFontScalingChanged(BindableObject bindable, object oldValue, object newValue)
		{
			if (GetCancelEvents(bindable))
				return;

			SetCancelEvents(bindable, true);

			var fontSize = (double)bindable.GetValue(FontSizeProperty);
			var fontAttributes = (FontAttributes)bindable.GetValue(FontAttributesProperty);
			var fontFamily = (string)bindable.GetValue(FontFamilyProperty);
			var enableAutoScaling = (bool)newValue;

			if (fontFamily != null)
				bindable.SetValue(FontProperty, Font.OfSize(fontFamily, fontSize, enableScaling: enableAutoScaling).WithAttributes(fontAttributes));
			else
				bindable.SetValue(FontProperty, Font.SystemFontOfSize(fontSize, enableScaling: enableAutoScaling).WithAttributes(fontAttributes));

			SetCancelEvents(bindable, false);
			((IFontElement)bindable).OnFontScalingEnableChanged((bool)oldValue, (bool)newValue);
		}
	}
}