using System;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.GradientGalleries
{
	public partial class UpdateGradientColorGallery : ContentPage
	{
		readonly Random _random;

		public UpdateGradientColorGallery()
		{
			InitializeComponent();
			_random = new Random();
		}

		void OnUpdateSolidColorClicked(object sender, EventArgs e)
		{
			SolidBrush.Color = GetRandomColor();
		}

		void OnUpdateLinearColorsClicked(object sender, EventArgs e)
		{
			GradientStop randomStop = LinearBrush.GradientStops[GetRandomGradientStop()];
			randomStop.Color = GetRandomColor();
		}

		void OnUpdateRadialColorsClicked(object sender, EventArgs e)
		{
			GradientStop firstStop = RadialBrush.GradientStops[GetRandomGradientStop()];
			firstStop.Color = GetRandomColor();
		}

		int GetRandomGradientStop()
		{
			return _random.Next(3);
		}

		Color GetRandomColor()
		{
			return Color.FromRgb(_random.Next(256), _random.Next(256), _random.Next(256));
		}
	}
}