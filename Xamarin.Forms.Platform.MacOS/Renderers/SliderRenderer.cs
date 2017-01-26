using System;
using SizeF = CoreGraphics.CGSize;
using AppKit;
using System.ComponentModel;

namespace Xamarin.Forms.Platform.MacOS
{
	public class SliderRenderer : ViewRenderer<Slider, NSSlider>
	{
		bool _disposed;

		IElementController ElementController => Element;

		protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new NSSlider { Continuous = true });
					Control.Activated += OnControlActivated;
				}

				UpdateMaximum();
				UpdateMinimum();
				UpdateValue();
			}

			base.OnElementChanged(e);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			if (e.PropertyName == Slider.MaximumProperty.PropertyName)
				UpdateMaximum();
			else if (e.PropertyName == Slider.MinimumProperty.PropertyName)
				UpdateMinimum();
			else if (e.PropertyName == Slider.ValueProperty.PropertyName)
				UpdateValue();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && !_disposed)
			{
				_disposed = true;
				if (Control != null)
					Control.Activated -= OnControlActivated;
			}

			base.Dispose(disposing);
		}

		void OnControlActivated(object sender, EventArgs eventArgs)
		{
			ElementController?.SetValueFromRenderer(Slider.ValueProperty, Control.DoubleValue);
		}

		void UpdateMaximum()
		{
			Control.MaxValue = (float)Element.Maximum;
		}

		void UpdateMinimum()
		{
			Control.MinValue = (float)Element.Minimum;
		}

		void UpdateValue()
		{
			if (Math.Abs(Element.Value - Control.DoubleValue) > 0)
				Control.DoubleValue = (float)Element.Value;
		}
	}
}