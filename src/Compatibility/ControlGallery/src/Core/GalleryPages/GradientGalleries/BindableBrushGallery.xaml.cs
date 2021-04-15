using System;
using Microsoft.Maui.Graphics;

namespace Microsoft.Maui.Controls.Compatibility.ControlGallery.GalleryPages.GradientGalleries
{
	public partial class BindableBrushGallery : ContentPage
	{
		public BindableBrushGallery()
		{
			InitializeComponent();
			InitializeBrush();
			BindingContext = this;
		}

		public LinearGradientBrush LinearGradient { get; set; }

		void InitializeBrush()
		{
			LinearGradient = new LinearGradientBrush
			{
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Red, Offset = 0.0f },
					new GradientStop { Color = Colors.Orange, Offset = 0.5f }
				},
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0)
			};
		}

		void OnUpdateBrushClicked(object sender, EventArgs e)
		{
			var random = new Random();

			LinearGradient.GradientStops = new GradientStopCollection
			{
				new GradientStop { Color = Color.FromRgb(random.Next(255), random.Next(255), random.Next(255)), Offset = 0.0f },
				new GradientStop { Color = Color.FromRgb(random.Next(255), random.Next(255), random.Next(255)), Offset = 0.5f }
			};
		}
	}
}