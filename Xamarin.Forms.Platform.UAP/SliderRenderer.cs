using System;
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Xamarin.Forms.Platform.UWP
{
	public class SliderRenderer : ViewRenderer<Slider, FormsSlider>
	{
		Brush defaultforegroundcolor;
		Brush defaultbackgroundcolor;
		Brush _defaultThumbColor;

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
					if (Element.VerticalOptions.Alignment == LayoutAlignment.Center && Control.Orientation == Windows.UI.Xaml.Controls.Orientation.Horizontal)
					{
						Control.VerticalAlignment = VerticalAlignment.Center;

						slider.Margin = new Windows.UI.Xaml.Thickness(0, 7, 0, 0);
					}
				}

				double stepping = Math.Min((e.NewElement.Maximum - e.NewElement.Minimum) / 10, 1);
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
			else if (e.PropertyName == Slider.ThumbImageProperty.PropertyName)
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

		void UpdateThumbImage()
		{
			if (Element == null || Control == null)
			{
				return;
			}

			var thumbImage = Element.ThumbImage;

			if (thumbImage == null)
			{
				Control.ThumbImage = null;
				return;
			}

			Control.ThumbImage = new BitmapImage(new Uri($"ms-appx:///{thumbImage.File}"));
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
					Control.ClearValue(Windows.UI.Xaml.Controls.Control.BackgroundProperty);
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
	}
}