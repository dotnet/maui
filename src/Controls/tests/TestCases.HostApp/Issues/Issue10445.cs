namespace Maui.Controls.Sample.Issues;

[Issue(IssueTracker.Github, 10445, "Shell.Background does not support gradient brushes", PlatformAffected.All)]
public class Issue10445 : TestShell
{
	protected override void Init()
	{
		FlyoutBehavior = FlyoutBehavior.Disabled;
		var gradientBrush = new LinearGradientBrush
		{
			StartPoint = new Point(0, 0),
			EndPoint = new Point(1, 1),
			GradientStops = new GradientStopCollection
			{
				new GradientStop(Colors.Yellow, 0.0f),
				new GradientStop(Colors.Green, 1.0f)
			}
		};

		Shell.SetBackground(this, gradientBrush);
		var gradientInfoLabel = new Label
		{
			Text = "Shell.Background should display a gradient (Yellow to Green) in the navigation bar above.",
			AutomationId = "GradientInfoLabel",
			HorizontalTextAlignment = TextAlignment.Center
		};
		var isSolidBackground = false;
		var toggleBackgroundGesture = new TapGestureRecognizer();
		toggleBackgroundGesture.Tapped += (sender, args) =>
		{
			if (isSolidBackground)
			{
				ClearValue(Shell.BackgroundProperty);
				gradientInfoLabel.Text = "Shell.Background should display the platform default in the navigation and tab bars.";
			}
			else
			{
				Shell.SetBackground(this, new SolidColorBrush(Colors.Blue));
				gradientInfoLabel.Text = "Shell.Background should display solid blue in the navigation and tab bars.";
			}

			isSolidBackground = !isSolidBackground;
		};
		gradientInfoLabel.GestureRecognizers.Add(toggleBackgroundGesture);

		var page = new ContentPage
		{
			Title = "Gradient Shell",
			Content = new VerticalStackLayout
			{
				Padding = 20,
				Spacing = 10,
				VerticalOptions = LayoutOptions.Center,
				HorizontalOptions = LayoutOptions.Center,
				Children =
				{
					gradientInfoLabel
				}
			}
		};

		AddBottomTab(page, "Home");
		AddBottomTab(new ContentPage
		{
			Title = "Settings",
			Content = new Label
			{
				Text = "Settings",
				HorizontalOptions = LayoutOptions.Center,
				VerticalOptions = LayoutOptions.Center
			}
		}, "Settings");
	}
}
