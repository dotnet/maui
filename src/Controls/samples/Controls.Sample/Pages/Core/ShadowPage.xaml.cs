using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class ShadowPage
	{
		public ShadowPage()
		{
			InitializeComponent();

			UpdateBackground();
			UpdateShadow();
		}

		void OnBackgroundChanged(object sender, TextChangedEventArgs e)
		{
			UpdateBackground();
		}

		void OnShadowColorChanged(object sender, TextChangedEventArgs e)
		{
			UpdateShadow();
		}

		void OnShadowOffsetXChanged(object sender, ValueChangedEventArgs e)
		{
			UpdateShadow();
		}

		void OnShadowOffsetYChanged(object sender, ValueChangedEventArgs e)
		{
			UpdateShadow();
		}

		void OnShadowRadiusChanged(object sender, ValueChangedEventArgs e)
		{
			UpdateShadow();
		}

		void OnShadowOpacityChanged(object sender, ValueChangedEventArgs e)
		{
			UpdateShadow();
		}

		void UpdateBackground()
		{
			var backgroundColor = GetColorFromString(FillColor.Text);

			FillColor.Background = new SolidColorBrush(backgroundColor);
			ShadowView.Fill = ShadowView.Stroke = new SolidColorBrush(backgroundColor);
		}

		void UpdateShadow()
		{
			var shadow = new Shadow();

			var shadowColor = GetColorFromString(ShadowColor.Text);
			ShadowColor.Background = new SolidColorBrush(shadowColor);

			shadow.Color = shadowColor;
			shadow.Offset = new Size((float)ShadowOffsetXSlider.Value, (float)ShadowOffsetYSlider.Value);
			shadow.Radius = (float)ShadowRadiusSlider.Value;
			shadow.Opacity = (float)ShadowOpacitySlider.Value;

			ShadowView.Shadow = ClippedShadowView.Shadow = shadow;
		}

		Color GetColorFromString(string value)
		{
			if (string.IsNullOrEmpty(value))
				return Colors.Transparent;

			try
			{
				return Color.FromArgb(value);
			}
			catch (Exception)
			{
				return Colors.Transparent;
			}
		}
	}
}