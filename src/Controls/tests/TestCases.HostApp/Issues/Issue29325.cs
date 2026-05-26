namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 29325, "Button Shadow Color Transparency Not Applied Correctly", PlatformAffected.Android)]
public class Issue29325 : ContentPage
{
	public Issue29325()
	{
		var verticalStackLayout = new VerticalStackLayout();

		var withoutAlphaOpacityButton = new Button
		{
			AutomationId = "withoutAlphaOpacityButton",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Text = "Button shadow color no Alpha nor opacity",
			Shadow = new Shadow
			{
				Brush = Colors.Blue,
				Offset = new Point(0, 12),
				Radius = 12,
			}
		};

		var alphaButton = new Button
		{
			AutomationId = "alphaButton",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Text = "Button shadow color with Alpha",
			Margin = new Thickness(0, 50, 0, 0),
			Shadow = new Shadow
			{
				Brush = Colors.Blue.WithAlpha(0.4f),
				Offset = new Point(0, 12),
				Radius = 12,
			}
		};

		var opacityButton = new Button
		{
			AutomationId = "opacityButton",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Text = "Button shadow color with opacity",
			Margin = new Thickness(0, 50, 0, 0),
			Shadow = new Shadow
			{
				Brush = Colors.Blue,
				Offset = new Point(0, 12),
				Radius = 12,
				Opacity = 0.4f
			}
		};

		var alphaOpacityButton = new Button
		{
			AutomationId = "alphaOpacityButton",
			HorizontalOptions = LayoutOptions.Center,
			VerticalOptions = LayoutOptions.Center,
			Text = "Button shadow color with alpha and opacity",
			Margin = new Thickness(0, 50, 0, 0),
			Shadow = new Shadow
			{
				Brush = Colors.Blue.WithAlpha(0.4f),
				Offset = new Point(0, 12),
				Radius = 12,
				Opacity = 0.4f
			}
		};

		// Add the Button to the VerticalStackLayout
		verticalStackLayout.Children.Add(withoutAlphaOpacityButton);
		verticalStackLayout.Children.Add(alphaButton);
		verticalStackLayout.Children.Add(opacityButton);
		verticalStackLayout.Children.Add(alphaOpacityButton);

		// Set the Content of the page
		Content = verticalStackLayout;
	}
}