//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using Android.App;
//using Android.Content;
//using Android.Content.PM;
//using Android.OS;
//using Android.Renderscripts;
//using Android.Runtime;
//using Android.Views;
//using Android.Widget;
//using Microsoft.Maui.Controls.ControlGallery.Issues;

using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.ControlGallery.GalleryPages.GradientGalleries
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