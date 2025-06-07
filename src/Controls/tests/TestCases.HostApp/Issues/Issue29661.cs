using System.Collections.ObjectModel;

namespace Maui.Controls.Sample.Issues;
[Issue(IssueTracker.Github, 29661, "[iOS, Mac] StrokeDashArray Property not Rendering", PlatformAffected.iOS)]
public class Issue29661 : ContentPage
{
	public Issue29661()
	{
		var label = new Label
		{
			Text = "Test for Border with StrokeDashArray",
			AutomationId = "StrokeDashArrayLabel",
			HorizontalOptions = LayoutOptions.Center,
			FontSize = 16
		};

		var border = new Border
		{
			HeightRequest = 200,
			WidthRequest = 200,
			BackgroundColor = Colors.LightGray,
			Stroke = Colors.Blue,
			StrokeThickness = 5,
			StrokeDashArray = new DoubleCollection { 10, 5 }
		};

		var layout = new VerticalStackLayout
		{
			Spacing = 25,
			Padding = new Thickness(30, 60, 30, 30),
			VerticalOptions = LayoutOptions.Center,
			Children = { label, border }
		};

		Content = layout;
	}
}