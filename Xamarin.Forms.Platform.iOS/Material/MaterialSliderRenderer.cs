using System;
using System.ComponentModel;
using CoreGraphics;
using MaterialComponents;
using UIKit;
using Xamarin.Forms;
using MSlider = MaterialComponents.Slider;

[assembly: ExportRenderer(typeof(Xamarin.Forms.Slider), typeof(Xamarin.Forms.Platform.iOS.Material.MaterialSliderRenderer), new[] { typeof(VisualRendererMarker.Material) })]

namespace Xamarin.Forms.Platform.iOS.Material
{
	public class MaterialSliderRenderer : ViewRenderer<Slider, MSlider>
	{
		SemanticColorScheme _defaultColorScheme;
		SemanticColorScheme _colorScheme;

		public MaterialSliderRenderer()
		{
			VisualElement.VerifyVisualFlagEnabled();
		}

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

		protected virtual SemanticColorScheme CreateColorScheme()
		{
			return MaterialColors.Light.CreateColorScheme();
		}

		protected virtual void ApplyTheme()
		{
			SliderColorThemer.ApplySemanticColorScheme(CreateColorScheme(), Control);

			// TODO: This is not very safe as Google may change the way it
			//       colors the control.
			//       Right now, this must always come after the theme as this
			//       is an unsupported operation.
			OverrideThemeColors();
		}

		protected override MSlider CreateNativeControl()
		{
			return new MSlider { StatefulApiEnabled = true };
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

		void UpdateMaximum()
		{
			Control.MaximumValue = (nfloat)Element.Maximum;
		}

		void UpdateMinimum()
		{
			Control.MinimumValue = (nfloat)Element.Minimum;
		}

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

			// TODO: Potentially override alpha to match material design.

			if (!minColor.IsDefault)
			{
				Control.SetTrackFillColor(minColor.ToUIColor(), UIControlState.Normal);

				// if no max color was specified, then use a shade of the min
				if (maxColor.IsDefault)
					Control.SetTrackBackgroundColor(MatchAlpha(minColor, Control.GetTrackBackgroundColor(UIControlState.Normal)), UIControlState.Normal);
			}

			if (!maxColor.IsDefault)
				Control.SetTrackBackgroundColor(maxColor.ToUIColor(), UIControlState.Normal);

			if (!thumbColor.IsDefault)
				Control.SetThumbColor(thumbColor.ToUIColor(), UIControlState.Normal);

			UIColor MatchAlpha(Color color, UIColor alphaColor)
			{
				alphaColor.GetRGBA(out _, out _, out _, out var a);
				return color.ToUIColor().ColorWithAlpha(a);
			}
		}

		void OnControlValueChanged(object sender, EventArgs eventArgs)
		{
			Element.SetValueFromRenderer(Slider.ValueProperty, Control.Value);
		}
	}
}