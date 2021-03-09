using System;
using System.ComponentModel;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using Microsoft.UI.Xaml.Media.Imaging;
using WBrush = Microsoft.UI.Xaml.Media.Brush;

namespace Microsoft.Maui.Controls.Compatibility.Platform.UWP
{
	public class SliderRenderer : ViewRenderer<Slider, FormsSlider>
	{
		WBrush defaultforegroundcolor;
		WBrush defaultbackgroundcolor;
		WBrush _defaultThumbColor;

		PointerEventHandler _pointerPressedHandler;
		PointerEventHandler _pointerReleasedHandler;

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				if (Control != null)
				{
					Control.RemoveHandler(PointerPressedEvent, _pointerPressedHandler);
					Control.RemoveHandler(PointerReleasedEvent, _pointerReleasedHandler);
					Control.RemoveHandler(PointerCanceledEvent, _pointerReleasedHandler);

					_pointerPressedHandler = null;
					_pointerReleasedHandler = null;
				}
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
		{
			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					var slider = new FormsSlider();
					SetNativeControl(slider);

					slider.Ready += (sender, args) =>
					{
						UpdateThumbColor();
						UpdateThumbImage();
					};

					Control.Minimum = e.NewElement.Minimum;
					Control.Maximum = e.NewElement.Maximum;
					Control.Value = e.NewElement.Value;
					Control.IsThumbToolTipEnabled = false;

					slider.ValueChanged += OnNativeValueChanged;

					defaultforegroundcolor = slider.Foreground;
					defaultbackgroundcolor = slider.Background;

					// Even when using Center/CenterAndExpand, a Slider has an oddity where it looks
					// off-center in its layout by a smidge. The default templates are slightly different
					// between 8.1/UWP; the 8.1 rows are 17/Auto/32 and UWP are 18/Auto/18. The value of
					// the hardcoded 8.1 rows adds up to 49 (when halved is 24.5) and UWP are 36 (18). Using
					// a difference of about 6 pixels to correct this oddity seems to make them both center
					// more correctly.
					//
					// The VerticalAlignment needs to be set as well since a control would not actually be
					// centered if a larger HeightRequest is set.
					if (Element.VerticalOptions.Alignment == LayoutAlignment.Center && Control.Orientation == Microsoft.UI.Xaml.Controls.Orientation.Horizontal)
					{
						Control.VerticalAlignment = VerticalAlignment.Center;

						slider.Margin = WinUIHelpers.CreateThickness(0, 7, 0, 0);
					}

					_pointerPressedHandler = new PointerEventHandler(OnPointerPressed);
					_pointerReleasedHandler = new PointerEventHandler(OnPointerReleased);

					Control.AddHandler(PointerPressedEvent, _pointerPressedHandler, true);
					Control.AddHandler(PointerReleasedEvent, _pointerReleasedHandler, true);
					Control.AddHandler(PointerCanceledEvent, _pointerReleasedHandler, true);
				}

				double stepping = Math.Min((e.NewElement.Maximum - e.NewElement.Minimum) / 1000, 1);
				Control.StepFrequency = stepping;
				Control.SmallChange = stepping;
				UpdateFlowDirection();
				UpdateSliderColors();
			}
		}

		void UpdateSliderColors()
		{
			UpdateMinimumTrackColor();
			UpdateMaximumTrackColor();
		}

		void UpdateMinimumTrackColor()
		{
			if (Control != null)
			{
				if (Element.MinimumTrackColor == Color.Default)
					Control.Foreground = defaultforegroundcolor;
				else
					Control.Foreground = Element.MinimumTrackColor.ToBrush();
			}
		}

		void UpdateMaximumTrackColor()
		{
			if (Control != null)
			{
				if (Element.MaximumTrackColor == Color.Default)
					Control.Background = defaultbackgroundcolor;
				else
					Control.Background = Element.MaximumTrackColor.ToBrush();
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
			else if (e.PropertyName == VisualElement.FlowDirectionProperty.PropertyName)
				UpdateFlowDirection();
			else if (e.PropertyName == Slider.MinimumTrackColorProperty.PropertyName)
				UpdateMinimumTrackColor();
			else if (e.PropertyName == Slider.MaximumTrackColorProperty.PropertyName)
				UpdateMaximumTrackColor();
			else if (e.PropertyName == Slider.ThumbColorProperty.PropertyName)
				UpdateThumbColor();
			else if (e.PropertyName == Slider.ThumbImageSourceProperty.PropertyName)
				UpdateThumbImage();
		}

		void UpdateThumbColor()
		{
			if (Element == null)
			{
				return;
			}

			var thumb = Control?.Thumb;

			if (thumb == null)
			{
				return;
			}
			
			BrushHelpers.UpdateColor(Element.ThumbColor, ref _defaultThumbColor, 
				() => thumb.Background, brush => thumb.Background = brush);
		}

		async void UpdateThumbImage()
		{
			if (Element == null || Control == null)
			{
				return;
			}

			var thumbImage = Element.ThumbImageSource;

			if (thumbImage == null)
			{
				Control.ThumbImageSource = null;
				return;
			}

			Control.ThumbImageSource = await thumbImage.ToWindowsImageSourceAsync();
		}

		protected override void UpdateBackgroundColor()
		{
			if (Control != null)
			{
				Color backgroundColor = Element.BackgroundColor;
				if (!backgroundColor.IsDefault)
				{
					Control.Background = backgroundColor.ToBrush();
				}
				else
				{
					Control.ClearValue(Microsoft.UI.Xaml.Controls.Control.BackgroundProperty);
				}
			}
		}

		protected override void UpdateBackground()
		{
			if (Control != null)
			{
				if (!Brush.IsNullOrEmpty(Element.Background))
				{
					Control.Background = Element.Background.ToBrush();
				}
				else
				{
					Color backgroundColor = Element.BackgroundColor;
					if (backgroundColor.IsDefault)
						Control.ClearValue(Microsoft.UI.Xaml.Controls.Control.BackgroundProperty);
				}
			}
		}

		void UpdateFlowDirection()
		{
			Control.UpdateFlowDirection(Element);
		}

		protected override bool PreventGestureBubbling { get; set; } = true;

		void OnNativeValueChanged(object sender, RangeBaseValueChangedEventArgs e)
		{
			((IElementController)Element).SetValueFromRenderer(Slider.ValueProperty, e.NewValue);
		}

		void OnPointerPressed(object sender, PointerRoutedEventArgs e)
		{
			((ISliderController)Element)?.SendDragStarted();
		}

		void OnPointerReleased(object sender, PointerRoutedEventArgs e)
		{
			((ISliderController)Element)?.SendDragCompleted();
		}
	}
}