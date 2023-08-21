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
	public partial class AnimateBrushGallery : ContentPage
	{
		readonly Random _random;

		public AnimateBrushGallery()
		{
			InitializeComponent();

			_random = new Random();

			UpdateBrush();

			Device.StartTimer(TimeSpan.FromSeconds(1), () =>
			{
				UpdateBrush();
				return true;
			});
		}

		void UpdateBrush()
		{
			var linearGradientBrush = new LinearGradientBrush
			{
				StartPoint = new Point(GetRandomInt(), GetRandomInt()),
				EndPoint = new Point(GetRandomInt(), GetRandomInt()),
				GradientStops = new GradientStopCollection
					{
						new GradientStop { Color = GetRandomColor() },
						new GradientStop { Color = GetRandomColor() }
					}
			};

			GradientView.Background = linearGradientBrush;
		}

		double GetRandomInt()
		{
			return _random.NextDouble() * 1;
		}

		Color GetRandomColor()
		{
			return Color.FromRgb(_random.Next(256), _random.Next(256), _random.Next(256));
		}
	}
}