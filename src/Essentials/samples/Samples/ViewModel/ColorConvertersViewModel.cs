using System;
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
			set => SetProperty(ref alpha, value, onChanged: SetColor);
		}

		public int Luminosity
		{
			get => luminosity;
			set => SetProperty(ref luminosity, value, onChanged: SetColor);
		}

		public int Hue
		{
			get => hue;
			set => SetProperty(ref hue, value, onChanged: SetColor);
		}

		public int Saturation
		{
			get => saturation;
			set => SetProperty(ref saturation, value, onChanged: SetColor);
		}

		public string Hex
		{
			get => hex;
			set => SetProperty(ref hex, value, onChanged: SetColor);
		}

		public Color RegularColor { get; set; }

		public Color AlphaColor { get; set; }

		public Color SaturationColor { get; set; }

		public Color HueColor { get; set; }

		public Color ComplementColor { get; set; }

		public Color LuminosityColor { get; set; }

		public string ComplementHex { get; set; }

		async void SetColor()
		{
			try
			{
				var color = Color.FromArgb(Hex);

				RegularColor = color;
				AlphaColor = color.WithAlpha(Alpha / 255f);
				SaturationColor = color.WithSaturation(Saturation / 100f);
				HueColor = color.WithHue(Hue / 255f);
				LuminosityColor = color.WithLuminosity(Luminosity / 100f);
				ComplementColor = color.GetComplementary();
				ComplementHex = ComplementColor.ToHex();

				OnPropertyChanged(nameof(RegularColor));
				OnPropertyChanged(nameof(AlphaColor));
				OnPropertyChanged(nameof(SaturationColor));
				OnPropertyChanged(nameof(HueColor));
				OnPropertyChanged(nameof(ComplementColor));
				OnPropertyChanged(nameof(LuminosityColor));
				OnPropertyChanged(nameof(ComplementHex));
			}
			catch (Exception ex)
			{
				await DisplayAlertAsync($"Unable to convert colors: {ex.Message}");
			}
		}
	}
}
