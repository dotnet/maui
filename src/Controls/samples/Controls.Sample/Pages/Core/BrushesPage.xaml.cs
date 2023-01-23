using System;
using System.Diagnostics;
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

		protected override void OnAppearing()
		{
			base.OnAppearing();

			Debug.WriteLine($"BindingContextBorder Parent {BindingContextBorder.Parent}, Brush Parent {BindingContextBorder.Background.Parent}");
		}

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

			if (LinearBrushBorder.Background is LinearGradientBrush linearBrushBorder)
			{
				GradientStop randomStop = linearBrushBorder.GradientStops[gradientStop];
				randomStop.Color = color;
			}

			if (LinearBrushPolygon.Fill is LinearGradientBrush linearBrushPolygon)
			{
				GradientStop randomStop = linearBrushPolygon.GradientStops[gradientStop];
				randomStop.Color = color;
			}
		}

		void OnRemovePolygonLinearColorsClicked(object sender, EventArgs e)
		{
			BrushChangesLayout.Children.Remove(LinearBrushPolygon);
		}

		void OnUpdateRadialColorsClicked(object sender, EventArgs e)
		{
			var gradientStop = GetRandomGradientStop();
			var color = GetRandomColor();

			if (RadialBrushBorder.Background is RadialGradientBrush radialGradientBrush)
			{
				GradientStop firstStop = radialGradientBrush.GradientStops[gradientStop];
				firstStop.Color = color;
			}

			if (RadialBrushPolygon.Fill is RadialGradientBrush radialBrushPolygon)
			{
				GradientStop firstStop = radialBrushPolygon.GradientStops[gradientStop];
				firstStop.Color = color;
			}
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