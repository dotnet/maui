using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages.ShapesGalleries
{
	public partial class InvalidateBrushGallery : ContentPage
	{
		SolidColorBrush _brush;
		readonly Color[] _colors = { Colors.Green, Colors.Red, Colors.Blue };
		int _colorIndex = -1;

		public InvalidateBrushGallery()
		{
			InitializeComponent();

			_brush = new SolidColorBrush();
			UpdateBrush();

			MyLine.Stroke = MyButton.Background = _brush;
		}

		protected override void OnAppearing()
		{
			base.OnAppearing();

			MyButton.Clicked += OnButtonClicked;
		}

		protected override void OnDisappearing()
		{
			base.OnDisappearing();

			MyButton.Clicked -= OnButtonClicked;
		}

		void OnButtonClicked(object? sender, System.EventArgs e)
		{
			UpdateBrush();
		}

		void UpdateBrush()
		{
			if (++_colorIndex >= _colors.Length)
				_colorIndex = 0;

			_brush.Color = _colors[_colorIndex];
		}
	}
}