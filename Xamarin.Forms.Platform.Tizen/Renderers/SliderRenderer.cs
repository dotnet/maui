using System;
using System.ComponentModel;
using ESlider = ElmSharp.Slider;
using ESize = ElmSharp.Size;

namespace Xamarin.Forms.Platform.Tizen
{
	public class SliderRenderer : ViewRenderer<Slider, ESlider>
	{
		protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
		{
			if (Control == null)
			{
				SetNativeControl(new ESlider(Forms.NativeParent)
				{
					PropagateEvents = false,
				});
				Control.ValueChanged += OnValueChanged;
			}
			UpdateMinimum();
			UpdateMaximum();
			UpdateValue();
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
			Element.Value = Control.Value;
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
	}
}
