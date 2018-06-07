using System;
using System.ComponentModel;
using ESlider = ElmSharp.Slider;
using ESize = ElmSharp.Size;
using EColor = ElmSharp.Color;

namespace Xamarin.Forms.Platform.Tizen
{
	public class SliderRenderer : ViewRenderer<Slider, ESlider>
	{
		EColor _defaultMinColor;
		EColor _defaultMaxColor;
		EColor _defaultThumbColor;

		protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
		{
			if (Control == null)
			{
				SetNativeControl(new ESlider(Forms.NativeParent));
				Control.ValueChanged += OnValueChanged;
				_defaultMinColor = Control.GetPartColor("bar");
				_defaultMaxColor = Control.GetPartColor("bg");
				_defaultThumbColor = Control.GetPartColor("handler");
			}
			UpdateMinimum();
			UpdateMaximum();
			UpdateValue();
			UpdateSliderColors();
			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			if (e.PropertyName == Slider.MinimumProperty.PropertyName)
			{
				UpdateMinimum();
			}
			else if (e.PropertyName == Slider.MaximumProperty.PropertyName)
			{
				UpdateMaximum();
			}
			else if (e.PropertyName == Slider.ValueProperty.PropertyName)
			{
				UpdateValue();
			}
			else if (e.PropertyName == Slider.MinimumTrackColorProperty.PropertyName)
			{
				UpdateMinimumTrackColor();
			}
			else if (e.PropertyName == Slider.MaximumTrackColorProperty.PropertyName)
			{
				UpdateMaximumTrackColor();
			}
			else if (e.PropertyName == Slider.ThumbColorProperty.PropertyName)
			{
				UpdateThumbColor();
			}
			base.OnElementPropertyChanged(sender, e);
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.ValueChanged -= OnValueChanged;
				}
			}
			base.Dispose(disposing);
		}

		protected override ESize Measure(int availableWidth, int availableHeight)
		{
			return new ESize(Math.Min(200, availableWidth), 50);
		}

		void OnValueChanged(object sender, EventArgs e)
		{
			Element.SetValueFromRenderer(Slider.ValueProperty, Control.Value);
		}

		protected void UpdateValue()
		{
			Control.Value = Element.Value;
		}

		protected void UpdateMinimum()
		{
			Control.Minimum = Element.Minimum;
		}

		protected void UpdateMaximum()
		{
			Control.Maximum = Element.Maximum;
		}

		protected void UpdateMinimumTrackColor()
		{
			var color = Element.MinimumTrackColor.IsDefault ? _defaultMinColor : Element.MinimumTrackColor.ToNative();
			Control.SetPartColor("bar", color);
			Control.SetPartColor("bar_pressed", color);
		}

		protected void UpdateMaximumTrackColor()
		{
			Control.SetPartColor("bg", Element.MaximumTrackColor.IsDefault ? _defaultMaxColor : Element.MaximumTrackColor.ToNative());
		}

		protected void UpdateThumbColor()
		{
			var color = Element.ThumbColor.IsDefault ? _defaultThumbColor : Element.ThumbColor.ToNative();
			Control.SetPartColor("handler", color);
			Control.SetPartColor("handler_pressed", color);
		}

		protected void UpdateSliderColors()
		{
			// Changing slider color is only available on mobile profile. Otherwise ignored.
			UpdateMinimumTrackColor();
			UpdateMaximum();
			UpdateThumbColor();
		}
	}
}
