using System;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace ShapesDemos.Views
{
    public partial class SpiralDemoPage : ContentPage
    {
        protected Polyline polyline;

        public SpiralDemoPage()
        {
            InitializeComponent();

            polyline = new Polyline
            {
                Stroke = Brush.Blue,
                StrokeThickness = 3
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

            for (double angle = 0; angle < 3600; angle +=1)
            {
                double scaledRadius = radius * angle / 3600;
                double radians = Math.PI * angle / 180;
                double x = center.X + scaledRadius * Math.Cos(radians);
                double y = center.Y + scaledRadius * Math.Sin(radians);
                points.Add(new Point(x, y));
            }

            polyline.Points = points;
			InvalidateMeasure();
		}
	}
}
