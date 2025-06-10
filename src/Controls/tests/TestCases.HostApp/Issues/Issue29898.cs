using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 29898, "[iOS, macOS] StrokeDashArray on Border does not reset when set to null", PlatformAffected.iOS)]
public class Issue29898 : ContentPage
{
	public Issue29898()
	{
		var border = new Border
		{
			HeightRequest = 200,
			WidthRequest = 200,
			BackgroundColor = Colors.LightGray,
			Stroke = Colors.Blue,
			StrokeThickness = 5,
			StrokeDashArray = new DoubleCollection { 10, 5 }
		};

		var button = new Button
		{
			Text = "Clear StrokeDashArray",
			AutomationId = "ClearDashButton",
			HorizontalOptions = LayoutOptions.Center
		};

		button.Clicked += (s, e) =>
		{
			border.StrokeDashArray = null;
		};

		var layout = new VerticalStackLayout
		{
			Spacing = 25,
			Padding = new Thickness(30, 60, 30, 30),
			VerticalOptions = LayoutOptions.Center,
			Children = { border, button }
		};

		Content = layout;
	}

}