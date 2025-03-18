using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 19668, "BoxView Placement inside Border.", PlatformAffected.UWP)]

public class Issue19668 : ContentPage
{
	public Issue19668()
	{
		var horizontalStackLayout = new HorizontalStackLayout
		{
			Spacing = 25,
			HorizontalOptions = LayoutOptions.Center
		};

		var rightBorder = new Border
		{
			WidthRequest = 55,
			HeightRequest = 35,
			StrokeThickness = 3,
			Background = Colors.Green,
			StrokeShape = new RoundRectangle
			{
				CornerRadius = 6
			}
		};

		horizontalStackLayout.Add(rightBorder);

		var plusLabel = new Label
		{
			Text = "+",
			AutomationId = "Label",
			VerticalOptions = LayoutOptions.Center
		};
		horizontalStackLayout.Add(plusLabel);

		var boxView = new BoxView
		{
			WidthRequest = 55,
			HeightRequest = 35,
			Color = Colors.LightBlue,
			CornerRadius = 6,
		};
		horizontalStackLayout.Add(boxView);

		var equalLabel = new Label
		{
			Text = "=",
			VerticalOptions = LayoutOptions.Center
		};
		horizontalStackLayout.Add(equalLabel);

		var parentBorder = new Border
		{
			WidthRequest = 55,
			HeightRequest = 35,
			StrokeThickness = 3,
			Background = Colors.Green,
			StrokeShape = new RoundRectangle
			{
				CornerRadius = 6
			}
		};

		// BoxView inside the Border
		var innerBoxView = new BoxView
		{
			WidthRequest = 55,
			HeightRequest = 35,
			Color = Colors.LightBlue,
			CornerRadius = 6,
			BackgroundColor = Colors.Transparent
		};

		parentBorder.Content = innerBoxView;
		horizontalStackLayout.Add(parentBorder);

		Content = horizontalStackLayout;
	}
}