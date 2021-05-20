using System;
using System.Windows.Input;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Essentials;
using Microsoft.Maui.Graphics;

namespace Samples.ViewModel
{
	public class ColorConvertersViewModel : BaseViewModel
	{
		int alpha = 100;
		int saturation = 100;
		int hue = 360;
		int luminosity = 100;
		string hex = "#3498db";

		public ColorConvertersViewModel()
		{
		}

		public int Alpha
		{
			get => alpha;
			set
			{
				SetProperty(ref alpha, value);
				SetColor();
			}
		}

		public int Luminosity
		{
			get => luminosity;
			set
			{
				SetProperty(ref luminosity, value);
				SetColor();
			}
		}

		public int Hue
		{
			get => hue;
			set
			{
				SetProperty(ref hue, value);
				SetColor();
			}
		}

		public int Saturation
		{
			get => saturation;
			set
			{
				SetProperty(ref saturation, value);
				SetColor();
			}
		}

		public string Hex
		{
			get => hex;
			set
			{
				SetProperty(ref hex, value);
				SetColor();
			}
		}

		public Color RegularColor { get; set; }

		public Color AlphaColor { get; set; }

		public Color SaturationColor { get; set; }

		public Color HueColor { get; set; }

		public Color ComplementColor { get; set; }

		public Color LuminosityColor { get; set; }

		void SetColor()
		{
			try
			{
				var color = Color.FromArgb(Hex);
				RegularColor = color;
				AlphaColor = color.WithAlpha(Alpha / 255f);
				SaturationColor = color.WithSaturation(Saturation / 255f);
				HueColor = color.WithHue(Hue / 255f);
				LuminosityColor = color.WithLuminosity(Luminosity / 255f);
				ComplementColor = color.GetComplementary();
				OnPropertyChanged(nameof(RegularColor));
				OnPropertyChanged(nameof(AlphaColor));
				OnPropertyChanged(nameof(SaturationColor));
				OnPropertyChanged(nameof(HueColor));
				OnPropertyChanged(nameof(ComplementColor));
				OnPropertyChanged(nameof(LuminosityColor));
			}
			catch (Exception)
			{
			}
		}
	}
}
