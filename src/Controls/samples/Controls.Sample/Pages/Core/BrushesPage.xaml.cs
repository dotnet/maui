using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class BrushesPage
	{
		readonly Random _random;

		public BrushesPage()
		{
			InitializeComponent();

			BindingContext = this;

			_random = new Random();
		}

		public Color Start { get; set; } = Colors.Red;
		public Color Stop { get; set; } = Colors.Orange;

		void OnUpdateSolidColorClicked(object sender, EventArgs e)
		{
			var color = GetRandomColor();

			if (SolidBrushBorder.Background is SolidColorBrush solidColorBrush)
				solidColorBrush.Color = color;

			if (SolidBrushPolygon.Fill is SolidColorBrush solidBrushPolygon)
				solidBrushPolygon.Color = color;
		}

		void OnRemovePolygonSolidColorClicked(object sender, EventArgs e)
		{
			BrushChangesLayout.Children.Remove(SolidBrushPolygon);
		}

		void OnUpdateLinearColorsClicked(object sender, EventArgs e)
		{
			var gradientStop = GetRandomGradientStop();
			var color = GetRandomColor();

			UpdateGradientStopColor(LinearBrushBorder.Background as LinearGradientBrush, gradientStop, color);
			UpdateGradientStopColor(LinearBrushPolygon.Fill as LinearGradientBrush, gradientStop, color);
		}

		void UpdateGradientStopColor(GradientBrush? gradientBrush, int index, Color color)
		{
			if (gradientBrush is null)
				return;

			GradientStop randomStop = gradientBrush.GradientStops[index];
			randomStop.Color = color;
		}

		void OnRemovePolygonLinearColorsClicked(object sender, EventArgs e)
		{
			BrushChangesLayout.Children.Remove(LinearBrushPolygon);
		}

		void OnUpdateRadialColorsClicked(object sender, EventArgs e)
		{
			var gradientStop = GetRandomGradientStop();
			var color = GetRandomColor();

			UpdateGradientStopColor(RadialBrushBorder.Background as RadialGradientBrush, gradientStop, color);
			UpdateGradientStopColor(RadialBrushPolygon.Fill as RadialGradientBrush, gradientStop, color);
		}

		void OnRemovePolygonRadialColorsClicked(object sender, EventArgs e)
		{
			BrushChangesLayout.Children.Remove(RadialBrushPolygon);
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
