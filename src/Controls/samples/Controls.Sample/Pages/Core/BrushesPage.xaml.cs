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
			if(SolidBrushBorder.Background is SolidColorBrush solidColorBrush)
				solidColorBrush.Color = GetRandomColor();
		}

		void OnUpdateLinearColorsClicked(object sender, EventArgs e)
		{
			if (LinearBrushBorder.Background is LinearGradientBrush linearGradientBrush)
			{
				GradientStop randomStop = linearGradientBrush.GradientStops[GetRandomGradientStop()];
				randomStop.Color = GetRandomColor();
			}
		}

		void OnUpdateRadialColorsClicked(object sender, EventArgs e)
		{
			if (RadialBrushBorder.Background is RadialGradientBrush radialGradientBrush)
			{
				GradientStop firstStop = radialGradientBrush.GradientStops[GetRandomGradientStop()];
				firstStop.Color = GetRandomColor();
			}
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