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

		Content = new VerticalStackLayout
		{
			Padding = 20,
			Children =
			{
				label,
				border
			}
		};
	}
}