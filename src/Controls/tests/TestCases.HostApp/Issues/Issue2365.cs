using System.ComponentModel;

namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 2365, "[Enhancement] BoxView handlers should use Paint", PlatformAffected.All)]
public class Issue2365 : TestContentPage
{
	protected override void Init()
	{
		var stackLayout = new StackLayout
		{
			Padding = new Thickness(20),
			Spacing = 20
		};

		// Create a gradient for the Fill property
		var gradient = new LinearGradientBrush
		{
			StartPoint = new Point(0, 0),
			EndPoint = new Point(1, 1)
		};
		gradient.GradientStops.Add(new GradientStop { Color = Colors.Blue, Offset = 0.0f });
		gradient.GradientStops.Add(new GradientStop { Color = Colors.Purple, Offset = 1.0f });

		var boxView = new BoxView
		{
			Color = Colors.Red,
			Fill = gradient,
			HeightRequest = 100,
			WidthRequest = 200,
			AutomationId = "TestBoxView"
		};

		var descriptionLabel = new Label
		{
			Text = "BoxView with Fill (gradient) and Color (red). When Fill is set to null, Color will take over.",
			FontSize = 14,
			TextColor = Colors.Black
		};

		var button = new Button
		{
			Text = "Set Fill to null",
			AutomationId = "SetFillNullButton"
		};

		button.Clicked += (sender, e) =>
		{
			boxView.Fill = null;
		};

		stackLayout.Children.Add(descriptionLabel);
		stackLayout.Children.Add(boxView);
		stackLayout.Children.Add(button);

		Content = stackLayout;
	}
}

