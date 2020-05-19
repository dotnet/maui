using System;
using System.ComponentModel;
using CoreGraphics;
using MaterialComponents;
using UIKit;
using System.Maui;
using System.Maui.Platform.iOS;
using MSlider = MaterialComponents.Slider;

namespace System.Maui.Material.iOS
{
	public class MaterialSliderRenderer : ViewRenderer<Slider, MSlider>
	{
		SemanticColorScheme _defaultColorScheme;
		SemanticColorScheme _colorScheme;
		protected override void Dispose(bool disposing)
		{
			if (Control != null)
			{
				Control.Delegate = null;
				Control.ValueChanged -= OnControlValueChanged;
			}

			base.Dispose(disposing);
		}

		protected override void OnElementChanged(ElementChangedEventArgs<Slider> e)
		{
			_colorScheme?.Dispose();
			_colorScheme = CreateColorScheme();

			base.OnElementChanged(e);

			if (e.NewElement != null)
			{
				if (Control == null)
				{
					_defaultColorScheme = CreateColorScheme();

					SetNativeControl(CreateNativeControl());

					Control.Continuous = true;
					Control.ValueChanged += OnControlValueChanged;
				}

				UpdateMaximum();
				UpdateMinimum();
				UpdateValue();

				ApplyTheme();
			}
		}

		protected virtual void ApplyTheme()
		{
#pragma warning disable CS0618 // Type or member is obsolete
			SliderColorThemer.ApplySemanticColorScheme(CreateColorScheme(), Control);
#pragma warning restore CS0618 // Type or member is obsolete

			// TODO: This is not very safe as Google may change the way it
			//       colors the control.
			//       Right now, this must always come after the theme as this
			//       is an unsupported operation.
			OverrideThemeColors();
		}


		public override CGSize SizeThatFits(CGSize size)
		{
			var result = base.SizeThatFits(size);

			var height = result.Height;
			if (height == 0)
				height = nfloat.IsInfinity(size.Height) ? 12 : size.Height;

			return new CGSize(12, height);
		}

		protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
		{
			base.OnElementPropertyChanged(sender, e);

			var updatedTheme = false;
			if (e.PropertyName == Slider.MaximumProperty.PropertyName)
			{
				UpdateMaximum();
			}
			else if (e.PropertyName == Slider.MinimumProperty.PropertyName)
			{
				UpdateMinimum();
			}
			else if (e.PropertyName == Slider.ValueProperty.PropertyName)
			{
				UpdateValue();
			}
			else if (e.PropertyName == Slider.MinimumTrackColorProperty.PropertyName || e.PropertyName == Slider.MaximumTrackColorProperty.PropertyName || e.PropertyName == Slider.ThumbColorProperty.PropertyName)
			{
				updatedTheme = true;
			}

			if (updatedTheme)
				ApplyTheme();
		}

		protected override MSlider CreateNativeControl() => new MSlider { StatefulApiEnabled = true };
		protected virtual SemanticColorScheme CreateColorScheme() => MaterialColors.Light.CreateColorScheme();

		void UpdateMaximum() => Control.MaximumValue = (nfloat)Element.Maximum;
		void UpdateMinimum() => Control.MinimumValue = (nfloat)Element.Minimum;
		void OnControlValueChanged(object sender, EventArgs eventArgs) => Element.SetValueFromRenderer(Slider.ValueProperty, Control.Value);

		void UpdateValue()
		{
			nfloat value = (nfloat)Element.Value;
			if (value != Control.Value)
				Control.Value = value;
		}

		void OverrideThemeColors()
		{
			Color minColor = Element.MinimumTrackColor;
			Color maxColor = Element.MaximumTrackColor;
			Color thumbColor = Element.ThumbColor;

			// jump out as we want the defaults
			if (minColor.IsDefault && maxColor.IsDefault && thumbColor.IsDefault)
				return;

			if (!minColor.IsDefault)
			{
				Control.SetTrackFillColor(minColor.ToUIColor(), UIControlState.Normal);

				// if no max color was specified, then use a shade of the min
				if (maxColor.IsDefault)
				{
					Control.GetTrackBackgroundColor(UIControlState.Normal).GetRGBA(out _, out _, out _, out var baseAlpha);
					Control.SetTrackBackgroundColor(minColor.MultiplyAlpha(baseAlpha).ToUIColor(), UIControlState.Normal);
				}

				if (thumbColor.IsDefault)
					Control.SetThumbColor(minColor.ToUIColor(), UIControlState.Normal);
			}

			if (!maxColor.IsDefault)
				Control.SetTrackBackgroundColor(maxColor.ToUIColor(), UIControlState.Normal);

			if (!thumbColor.IsDefault)
				Control.SetThumbColor(thumbColor.ToUIColor(), UIControlState.Normal);
		}
	}
}