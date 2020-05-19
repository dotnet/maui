using System;
using System.ComponentModel;
using UIKit;
using SizeF = CoreGraphics.CGSize;
using Xamarin.Forms.PlatformConfiguration.iOSSpecific;
using Specifics = Xamarin.Forms.PlatformConfiguration.iOSSpecific.Slider;

namespace Xamarin.Forms.Platform.iOS
{
	public class SliderRenderer : ViewRenderer<Slider, UISlider>
	{
		SizeF _fitSize;
		UIColor defaultmintrackcolor, defaultmaxtrackcolor, defaultthumbcolor;
		UITapGestureRecognizer _sliderTapRecognizer;
		bool _disposed;

		[Internals.Preserve(Conditional = true)]
		public SliderRenderer()
		{

		}

		public override SizeF SizeThatFits(SizeF size)
		{
			return _fitSize;
		}

		protected override void Dispose(bool disposing)
		{
			if (_disposed)
			{
				return;
			}

			_disposed = true;

			if (disposing)
			{
				if (Control != null)
				{
					Control.ValueChanged -= OnControlValueChanged;
					if (_sliderTapRecognizer != null)
					{
						Control.RemoveGestureRecognizer(_sliderTapRecognizer);
						_sliderTapRecognizer = null;
					}

					Control.RemoveTarget(OnTouchDownControlEvent, UIControlEvent.TouchDown);
					Control.RemoveTarget(OnTouchUpControlEvent, UIControlEvent.TouchUpInside | UIControlEvent.TouchUpOutside);
				}
			}

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

					defaultmintrackcolor = Control.MinimumTrackTintColor;
					defaultmaxtrackcolor = Control.MaximumTrackTintColor;
					defaultthumbcolor = Control.ThumbTintColor;

					// except if your not running iOS 7... then it fails...
					if (_fitSize.Width <= 0 || _fitSize.Height <= 0)
						_fitSize = new SizeF(22, 22); // Per the glorious documentation known as the SDK docs

					Control.AddTarget(OnTouchDownControlEvent, UIControlEvent.TouchDown);
					Control.AddTarget(OnTouchUpControlEvent, UIControlEvent.TouchUpInside | UIControlEvent.TouchUpOutside);
				}

				UpdateMaximum();
				UpdateMinimum();
				UpdateValue();
				UpdateSliderColors();
				UpdateTapRecognizer();
			}

			base.OnElementChanged(e);
		}

		private void UpdateSliderColors()
		{
			UpdateMinimumTrackColor();
			UpdateMaximumTrackColor();
			var thumbImage = Element.ThumbImageSource;
			if (thumbImage != null && !thumbImage.IsEmpty)
			{
				UpdateThumbImage();
			}
			else
			{
				UpdateThumbColor();
			}
		}

		private void UpdateMinimumTrackColor()
		{
			if (Element != null)
			{
				if (Element.MinimumTrackColor == Color.Default)
					Control.MinimumTrackTintColor = defaultmintrackcolor;
				else
					Control.MinimumTrackTintColor = Element.MinimumTrackColor.ToUIColor();
			}
		}

		private void UpdateMaximumTrackColor()
		{
			if (Element != null)
			{
				if (Element.MaximumTrackColor == Color.Default)
					Control.MaximumTrackTintColor = defaultmaxtrackcolor;
				else
					Control.MaximumTrackTintColor = Element.MaximumTrackColor.ToUIColor();
			}
		}

		private void UpdateThumbColor()
		{
			if (Element != null)
			{
				if (Element.ThumbColor == Color.Default)
					Control.ThumbTintColor = defaultthumbcolor;
				else
					Control.ThumbTintColor = Element.ThumbColor.ToUIColor();
			}
		}

		void UpdateThumbImage()
		{
			_ = this.ApplyNativeImageAsync(Slider.ThumbImageSourceProperty, uiimage =>
			{
				Control?.SetThumbImage(uiimage, UIControlState.Normal);

				((IVisualElementController)Element).NativeSizeChanged();
			});
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
			else if (e.PropertyName == Slider.MinimumTrackColorProperty.PropertyName)
				UpdateMinimumTrackColor();
			else if (e.PropertyName == Slider.MaximumTrackColorProperty.PropertyName)
				UpdateMaximumTrackColor();
			else if (e.PropertyName == Slider.ThumbImageSourceProperty.PropertyName)
				UpdateThumbImage();
			else if (e.PropertyName == Slider.ThumbColorProperty.PropertyName)
				UpdateThumbColor();
			else if (e.PropertyName == Specifics.UpdateOnTapProperty.PropertyName)
				UpdateTapRecognizer();
		}

		void OnControlValueChanged(object sender, EventArgs eventArgs)
		{
			((IElementController)Element).SetValueFromRenderer(Slider.ValueProperty, Control.Value);
		}

		void OnTouchDownControlEvent(object sender, EventArgs e)
		{
			((ISliderController)Element)?.SendDragStarted();
		}

		void OnTouchUpControlEvent(object sender, EventArgs e)
		{
			((ISliderController)Element)?.SendDragCompleted();
		}

		void UpdateTapRecognizer()
		{
			if (Element != null && Element.IsSet(Specifics.UpdateOnTapProperty))
			{
				if (Element.OnThisPlatform().GetUpdateOnTap())
				{
					if (_sliderTapRecognizer == null)
					{
						_sliderTapRecognizer = new UITapGestureRecognizer((recognizer) =>
						{
							var control = Control;
							if (control != null)
							{
								var tappedLocation = recognizer.LocationInView(control);
								if (tappedLocation != null)
								{
									var val = (tappedLocation.X - control.Frame.X) * control.MaxValue / control.Frame.Size.Width;
									Element.SetValueFromRenderer(Slider.ValueProperty, val);
								}
							}
						});
						Control.AddGestureRecognizer(_sliderTapRecognizer);
					}
				}
				else
				{
					if (_sliderTapRecognizer != null)
					{
						Control.RemoveGestureRecognizer(_sliderTapRecognizer);
						_sliderTapRecognizer = null;
					}
				}
			}

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