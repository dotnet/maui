using Microsoft.Maui.Controls.Internals;

namespace Microsoft.Maui.Controls
{
	static class FontElement
	{
		public static readonly BindableProperty FontFamilyProperty =
			BindableProperty.Create("FontFamily", typeof(string), typeof(IFontElement), default(string),
									propertyChanged: OnFontFamilyChanged);

		public static readonly BindableProperty FontSizeProperty =
			BindableProperty.Create("FontSize", typeof(double), typeof(IFontElement), 0d,
									propertyChanged: OnFontSizeChanged,
									defaultValueCreator: FontSizeDefaultValueCreator);

		public static readonly BindableProperty FontAttributesProperty =
			BindableProperty.Create("FontAttributes", typeof(FontAttributes), typeof(IFontElement), FontAttributes.None,
									propertyChanged: OnFontAttributesChanged);

		public static readonly BindableProperty FontAutoScalingEnabledProperty =
			BindableProperty.Create("FontAutoScalingEnabled", typeof(bool), typeof(IFontElement), true,
									propertyChanged: OnFontAutoScalingEnabledChanged);

		static void OnFontFamilyChanged(BindableObject bindable, object oldValue, object newValue)
			=> ((IFontElement)bindable).OnFontFamilyChanged((string)oldValue, (string)newValue);

		static void OnFontSizeChanged(BindableObject bindable, object oldValue, object newValue)
			=> ((IFontElement)bindable).OnFontSizeChanged((double)oldValue, (double)newValue);

		static object FontSizeDefaultValueCreator(BindableObject bindable)
			=> ((IFontElement)bindable).FontSizeDefaultValueCreator();

		static void OnFontAttributesChanged(BindableObject bindable, object oldValue, object newValue)
			=> ((IFontElement)bindable).OnFontAttributesChanged((FontAttributes)oldValue, (FontAttributes)newValue);

		static void OnFontAutoScalingEnabledChanged(BindableObject bindable, object oldValue, object newValue)
			=> ((IFontElement)bindable).OnFontAutoScalingEnabledChanged((bool)oldValue, (bool)newValue);
	}
}
