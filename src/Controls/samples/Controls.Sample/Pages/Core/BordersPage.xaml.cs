using System;
using Microsoft.Maui;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Converters;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Pages
{
	public partial class BordersPage
	{
		public BordersPage()
		{
			InitializeComponent();

			BorderShapePicker.SelectedIndex = 1;

			UpdateBackground();
			UpdateBorder();
			UpdateCornerRadius();
		}

		public Type SelectedView { get; set; }

		void OnBorderShapeSelectedIndexChanged(object sender, EventArgs e)
		{
			UpdateBorderShape();
		}

		void OnBackgroundChanged(object sender, TextChangedEventArgs e)
		{
			UpdateBackground();
		}

		void OnBorderChanged(object sender, TextChangedEventArgs e)
		{
			UpdateBorder();
		}

		void OnBorderWidthChanged(object sender, ValueChangedEventArgs e)
		{
			UpdateBorder();
		}

		void OnBorderDashArrayChanged(object sender, TextChangedEventArgs e)
		{
			UpdateBorder();
		}

		void OnBorderDashOffsetChanged(object sender, ValueChangedEventArgs e)
		{
			UpdateBorder();
		}

		void OnCornerRadiusChanged(object sender, ValueChangedEventArgs e)
		{
			UpdateCornerRadius();
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

		void UpdateBorder()
		{
			var startColor = GetColorFromString(BorderStartColor.Text);
			var endColor = GetColorFromString(BorderEndColor.Text);

			BorderStartColor.BackgroundColor = startColor;
			BorderEndColor.BackgroundColor = endColor;

			Shape borderShape = null;

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

			BorderView.BorderShape = borderShape;

			BorderView.BorderBrush = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 0),
				GradientStops = new GradientStopCollection
				{
					new Microsoft.Maui.Controls.GradientStop { Color = startColor, Offset = 0.0f },
					new Microsoft.Maui.Controls.GradientStop { Color = endColor, Offset = 0.9f }
				}
			};

			BorderView.BorderWidth = BorderWidthSlider.Value;

			var borderDashArrayString = BorderDashArrayEntry.Text;

			if (string.IsNullOrEmpty(borderDashArrayString))
				BorderView.BorderDashArray = new DoubleCollection();
			else
			{
				var doubleCollectionConverter = new DoubleCollectionConverter();
				var doubleCollection = (DoubleCollection)doubleCollectionConverter.ConvertFromString(borderDashArrayString);
				BorderView.BorderDashArray = doubleCollection;
			}

			BorderView.BorderDashOffset = BorderDashOffsetSlider.Value;
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