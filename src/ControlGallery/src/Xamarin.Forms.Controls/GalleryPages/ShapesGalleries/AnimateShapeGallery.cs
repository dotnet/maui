using System;
using Xamarin.Forms.Shapes;

namespace Xamarin.Forms.Controls.GalleryPages.ShapesGalleries
{
	public class SpiralDemoPage : ContentPage
	{
		protected Polyline polyline;

		public SpiralDemoPage()
		{
			var strokeBrush = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Color.Orange, Offset = 0.2f },
					new GradientStop { Color = Color.OrangeRed, Offset = 0.8f }
				}
			};

			polyline = new Polyline
			{
				Stroke = strokeBrush,
				StrokeThickness = 5
			};
			Content = polyline;

			SizeChanged += OnPageSizeChanged;
		}

		void OnPageSizeChanged(object sender, EventArgs e)
		{
			if (Width <= 0 || Height <= 0)
			{
				return;
			}

			polyline.Points.Clear();

			double radius = Math.Min(Width / 2, Height / 2);
			Point center = new Point(Width / 2, Height / 2);

			PointCollection points = polyline.Points;
			polyline.Points = null;

			for (double angle = 0; angle < 3600; angle += 1)
			{
				double scaledRadius = radius * angle / 3600;
				double radians = Math.PI * angle / 180;
				double x = center.X + scaledRadius * Math.Cos(radians);
				double y = center.Y + scaledRadius * Math.Sin(radians);
				points.Add(new Point(x, y));
			}

			polyline.Points = points;
		}
	}

	public class AnimateShapeGallery : SpiralDemoPage
	{
		public AnimateShapeGallery()
		{
			Title = "Animate Shape Gallery";

			polyline.StrokeDashArray.Add(4);
			polyline.StrokeDashArray.Add(2);
			double total = polyline.StrokeDashArray[0] + polyline.StrokeDashArray[1];

			Device.StartTimer(TimeSpan.FromMilliseconds(15), () =>
			{
				double secs = DateTime.Now.TimeOfDay.TotalSeconds;
				polyline.StrokeDashOffset = total * (secs % 1);
				return true;
			});
		}
	}
}