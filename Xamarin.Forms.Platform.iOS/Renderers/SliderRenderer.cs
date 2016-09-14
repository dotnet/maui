using System;
using System.ComponentModel;
using UIKit;
using SizeF = CoreGraphics.CGSize;

namespace Xamarin.Forms.Platform.iOS
{
	public class SliderRenderer : ViewRenderer<Slider, UISlider>
	{
		SizeF _fitSize;

		public override SizeF SizeThatFits(SizeF size)
		{
			return _fitSize;
		}

		protected override void Dispose(bool disposing)
		{
			if (Control != null)
				Control.ValueChanged -= OnControlValueChanged;

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
		{
			if (e.NewElement != null)
			{
				if (Control == null)
				{
					SetNativeControl(new UISlider { Continuous = true });
					Control.ValueChanged += OnControlValueChanged;

					// sliders SizeThatFits method returns non-useful answers
					// this however gives a very useful answer
					Control.SizeToFit();
					_fitSize = Control.Bounds.Size;

					// except if your not running iOS 7... then it fails...
					if (_fitSize.Width <= 0 || _fitSize.Height <= 0)
						_fitSize = new SizeF(22, 22); // Per the glorious documentation known as the SDK docs
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

		void OnControlValueChanged(object sender, EventArgs eventArgs)
		{
			((IElementController)Element).SetValueFromRenderer(Slider.ValueProperty, Control.Value);
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
			if ((float)Element.Value != Control.Value)
				Control.Value = (float)Element.Value;
		}
	}
}