#nullable enable
using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class BorderPlayground
	{
		public BorderPlayground()
		{
			InitializeComponent();

			BorderContentPicker.SelectedIndex = 0;
			BorderShapePicker.SelectedIndex = 1;
			BorderLineJoinPicker.SelectedIndex = 0;
			BorderLineCapPicker.SelectedIndex = 0;

			UpdateBackground();
			UpdateContentBackground();
			UpdateBorder();
			UpdateCornerRadius();
		}

		void OnBorderContentSelectedIndexChanged(object? sender, EventArgs e)
		{
			UpdateBorderContent();
			UpdateBorder();
		}

		void OnBorderShapeSelectedIndexChanged(object? sender, EventArgs e)
		{
			UpdateBorderShape();
		}

		void OnBorderLineJoinSelectedIndexChanged(object? sender, EventArgs e)
		{
			UpdateBorderShape();
		}

		void OnBorderLineCapSelectedIndexChanged(object? sender, EventArgs e)
		{
			UpdateBorderShape();
		}

		void OnBackgroundChanged(object? sender, TextChangedEventArgs e)
		{
			UpdateBackground();
		}

		void OnBorderChanged(object? sender, TextChangedEventArgs e)
		{
			UpdateBorder();
		}

		void OnBorderWidthChanged(object? sender, ValueChangedEventArgs e)
		{
			UpdateBorder();
		}

		void OnBorderDashArrayChanged(object? sender, TextChangedEventArgs e)
		{
			UpdateBorder();
		}

		void OnBorderDashOffsetChanged(object? sender, ValueChangedEventArgs e)
		{
			UpdateBorder();
		}

		void OnCornerRadiusChanged(object? sender, ValueChangedEventArgs e)
		{
			UpdateCornerRadius();
		}

		void OnContentBackgroundCheckBoxChanged(object? sender, CheckedChangedEventArgs e)
		{
			UpdateContentBackground();
		}

		void UpdateBorderShape()
		{
			CornerRadiusLayout.IsVisible = BorderShapePicker.SelectedIndex == 1;

			UpdateBorder();
		}

		void UpdateBackground()
		{
			var startColor = GetColorFromString(BackgroundStartColor.Text);
			var endColor = GetColorFromString(BackgroundEndColor.Text);

			BackgroundStartColor.BackgroundColor = startColor;
			BackgroundEndColor.BackgroundColor = endColor;

			BorderView.Background = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new Microsoft.Maui.Controls.GradientStop { Color = startColor, Offset = 0.0f },
					new Microsoft.Maui.Controls.GradientStop { Color = endColor, Offset = 0.9f }
				}
			};
		}

		void UpdateContentBackground()
		{
			if (BorderView.Content is View content)
				content.BackgroundColor = ContentBackgroundCheckBox.IsChecked ? Color.FromArgb("#99FF0000") : Colors.Transparent;
		}

		void UpdateBorderContent()
		{
			View? content = null;

			switch (BorderContentPicker.SelectedIndex)
			{
				case 0:
					content = new Label { Text = "Just a Label", FontSize = 20 };
					break;
				case 1:
					content = new Image { Aspect = Aspect.AspectFill, Source = "oasis.jpg" };
					break;
			}

			BorderView.Content = content;
		}

		void UpdateBorder()
		{
			var startColor = GetColorFromString(BorderStartColor.Text);
			var endColor = GetColorFromString(BorderEndColor.Text);

			BorderStartColor.BackgroundColor = startColor;
			BorderEndColor.BackgroundColor = endColor;

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

			BorderView.Stroke = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new Microsoft.Maui.Controls.GradientStop { Color = startColor, Offset = 0.0f },
					new Microsoft.Maui.Controls.GradientStop { Color = endColor, Offset = 0.9f }
				}
			};

			BorderView.StrokeThickness = BorderWidthSlider.Value;

			var borderDashArrayString = BorderDashArrayEntry.Text;

			if (string.IsNullOrEmpty(borderDashArrayString))
				BorderView.StrokeDashArray = new DoubleCollection();
			else
			{
				var doubleCollectionConverter = new DoubleCollectionConverter();
				var doubleCollection = doubleCollectionConverter.ConvertFromString(borderDashArrayString) as DoubleCollection;
				BorderView.StrokeDashArray = doubleCollection;
			}

			BorderView.StrokeDashOffset = BorderDashOffsetSlider.Value;

			PenLineJoin borderLineJoin = PenLineJoin.Miter;

			switch (BorderLineJoinPicker.SelectedIndex)
			{
				case 0:
					borderLineJoin = PenLineJoin.Miter;
					break;
				case 1:
					borderLineJoin = PenLineJoin.Round;
					break;
				case 2:
					borderLineJoin = PenLineJoin.Bevel;
					break;
			}

			BorderView.StrokeLineJoin = borderLineJoin;

			PenLineCap borderLineCap = PenLineCap.Flat;

			switch (BorderLineCapPicker.SelectedIndex)
			{
				case 0:
					borderLineCap = PenLineCap.Flat;
					break;
				case 1:
					borderLineCap = PenLineCap.Round;
					break;
				case 2:
					borderLineCap = PenLineCap.Square;
					break;
			}

			BorderView.StrokeLineCap = borderLineCap;
		}

		void UpdateCornerRadius()
		{
			UpdateBorder();
		}

		Color GetColorFromString(string value)
		{
			if (string.IsNullOrEmpty(value))
				return Colors.Transparent;

			try
			{
				return Color.FromArgb(value);
			}
			catch (Exception)
			{
				return Colors.Transparent;
			}
		}
	}
}