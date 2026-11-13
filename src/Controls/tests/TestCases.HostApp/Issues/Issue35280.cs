using Microsoft.Maui.Controls;
using Microsoft.Maui.Controls.Shapes;
using Microsoft.Maui.Graphics;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 35280, "LinearGradientBrush with transparent stops renders as opaque black box on Android", PlatformAffected.Android)]
public class Issue35280 : ContentPage
{
	public Issue35280()
	{
		Label label = new Label
		{
			AutomationId = "DescriptionLabel",
			Text = "The image below should have a gradient overlay that fades from black to transparent. If the image is completely covered by a black box, then the test has failed."
		};

		var border = new Border
		{
			Stroke = Colors.Transparent,
			Margin = new Thickness(5, 0, 0, 0),
			WidthRequest = 250,
			StrokeShape = new RoundRectangle
			{
				CornerRadius = new CornerRadius(10)
			},
			Content = new Grid
			{
				Children =
				{
					new Image
					{
						Source = "dotnet_bot.png",
						Aspect = Aspect.AspectFill
					},
					new VerticalStackLayout
					{
						VerticalOptions = LayoutOptions.End,
						Padding = 10,
						Background = new LinearGradientBrush
						{
							StartPoint = new Point(0, 1),
							EndPoint = new Point(1, 0),
							GradientStops = new GradientStopCollection
							{
								new GradientStop { Color = Colors.Black, Offset = 0.0f },
								new GradientStop { Color = Color.FromArgb("#80000000"), Offset = 0.5f },
								new GradientStop { Color = Colors.Transparent, Offset = 1.0f }
							}
						},
						Children =
						{
							new Label
							{
								Text = "Title",
								TextColor = Colors.White
							},
							new Label
							{
								Text = "Subtitle",
								TextColor = Colors.White
							}
						}
					}
				}
			}
		};

		// Radial gradient with transparent stops — exercises SetBackground(RadialGradientPaint)
		var radialGradientBox = new BoxView
		{
			AutomationId = "RadialGradientBox",
			WidthRequest = 250,
			HeightRequest = 100,
			Background = new RadialGradientBrush
			{
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Transparent, Offset = 0.0f },
					new GradientStop { Color = Color.FromArgb("#80000000"), Offset = 0.5f },
					new GradientStop { Color = Colors.Blue, Offset = 1.0f }
				}
			}
		};

		// Border with a LinearGradientBrush Stroke that fades to transparent — exercises SetBorderBrush(LinearGradientPaint)
		var borderGradientBox = new Border
		{
			AutomationId = "BorderGradientBox",
			WidthRequest = 250,
			HeightRequest = 100,
			StrokeThickness = 10,
			StrokeShape = new RoundRectangle
			{
				CornerRadius = new CornerRadius(10)
			},
			Stroke = new LinearGradientBrush
			{
				StartPoint = new Point(0, 0),
				EndPoint = new Point(1, 1),
				GradientStops = new GradientStopCollection
				{
					new GradientStop { Color = Colors.Purple, Offset = 0.0f },
					new GradientStop { Color = Color.FromArgb("#80000000"), Offset = 0.5f },
					new GradientStop { Color = Colors.Transparent, Offset = 1.0f }
				}
			},
			Content = new Label
			{
				Text = "Gradient border",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		};

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Children =
			{
				label,
				border,
				radialGradientBox,
				borderGradientBox
			}
		};
	}
}