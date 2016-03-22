using System;
using System.ComponentModel;
using Windows.UI.Xaml.Controls.Primitives;

#if WINDOWS_UWP

namespace Xamarin.Forms.Platform.UWP
#else

namespace Xamarin.Forms.Platform.WinRT
#endif
{
	public class SliderRenderer : ViewRenderer<Slider, Windows.UI.Xaml.Controls.Slider>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var slider = new Windows.UI.Xaml.Controls.Slider();
					SetNativeControl(slider);

					slider.ValueChanged += OnNativeValueCHanged;
				}

				double stepping = Math.Min((e.NewElement.Maximum - e.NewElement.Minimum) / 10, 1);
				Control.StepFrequency = stepping;
				Control.SmallChange = stepping;

				Control.Minimum = e.NewElement.Minimum;
				Control.Maximum = e.NewElement.Maximum;
				Control.Value = e.NewElement.Value;
			}
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Slider.MinimumProperty.PropertyName)
				Control.Minimum = Element.Minimum;
			else if (e.PropertyName == Slider.MaximumProperty.PropertyName)
				Control.Maximum = Element.Maximum;
			else if (e.PropertyName == Slider.ValueProperty.PropertyName)
			{
				if (Control.Value != Element.Value)
					Control.Value = Element.Value;
			}
		}

		void OnNativeValueCHanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			((IElementController)Element).SetValueFromRenderer(Slider.ValueProperty, e.NewValue);
		}
	}
}