using Microsoft.Maui.Controls.Shapes;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 11404, "Line coordinates not computed correctly", PlatformAffected.iOS | PlatformAffected.Android)]
public class Issue11404 : ContentPage
{
	public Issue11404()
	{
		Title = "Issue11404";

		var grid = new Grid
		{
			WidthRequest = 200,
			HeightRequest = 200,
			BackgroundColor = Colors.LightGray
		};

		var line1 = new Line
		{
			X1 = 0,
			Y1 = 0,
			X2 = 100,
			Y2 = 100,
			Stroke = new SolidColorBrush(Colors.Red),
			StrokeThickness = 10
		};

		var line2 = new Line
		{
			X1 = 200,
			Y1 = 0,
			X2 = 100,
			Y2 = 100,
			Stroke = new SolidColorBrush(Colors.Red),
			StrokeThickness = 10
		};

		var line3 = new Line
		{
			X1 = 200,
			Y1 = 0,
			X2 = 100,
			Y2 = 100,
			Stroke = new SolidColorBrush(Colors.Black),
			StrokeThickness = 1
		};

		var DescriptionLabel = new Label
		{
			Text = "There should be two thick red lines forming a V shape with a thin black line down the center.",
			Margin = new Thickness(10),
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.End,
			AutomationId = "DescriptionLabel"
		};

		grid.Children.Add(line1);
		grid.Children.Add(line2);
		grid.Children.Add(line3);
		grid.Children.Add(DescriptionLabel);
		Content = grid;
	}
}
