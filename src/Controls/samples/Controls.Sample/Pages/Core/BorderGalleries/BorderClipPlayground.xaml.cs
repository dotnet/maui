#nullable enable
using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Pages
{
	public partial class BorderClipPlayground
	{
		public BorderClipPlayground()
		{
			InitializeComponent();

			BorderShapePicker.SelectedIndex = 1;

			UpdateBorder();
			UpdateCornerRadius();
		}

		void OnBorderShapeSelectedIndexChanged(object? sender, EventArgs e)
		{
			UpdateBorderShape();
		}

		void OnBorderChanged(object? sender, TextChangedEventArgs e)
		{
			UpdateBorder();
		}

		void OnBorderWidthChanged(object? sender, ValueChangedEventArgs e)
		{
			UpdateBorder();
		}

		void OnCornerRadiusChanged(object? sender, ValueChangedEventArgs e)
		{
			UpdateCornerRadius();
		}

		void UpdateBorderShape()
		{
			CornerRadiusLayout.IsVisible = BorderShapePicker.SelectedIndex == 1;

			UpdateBorder();
		}

		void UpdateBorder()
		{
			Shape? borderShape = null;

			switch (BorderShapePicker.SelectedIndex)
			{
				case 0:
					borderShape = new Microsoft.Maui.Controls.Shapes.Rectangle();
					break;
				case 1:
					borderShape = new RoundRectangle
					{
						CornerRadius = new CornerRadius(TopLeftCornerSlider.Value, TopRightCornerSlider.Value,
						BottomLeftCornerSlider.Value, BottomRightCornerSlider.Value)
					};
					break;
				case 2:
					borderShape = new Ellipse();
					break;
			}

			BorderView.StrokeShape = borderShape;

			BorderView.StrokeThickness = BorderWidthSlider.Value;
		}

		void UpdateCornerRadius()
		{
			UpdateBorder();
		}
	}
}